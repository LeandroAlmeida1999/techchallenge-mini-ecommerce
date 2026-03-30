param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$stateDir = Join-Path $PSScriptRoot '.state'
$stateFile = Join-Path $stateDir 'port-forwards.json'

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Stop-PortForwards {
    if (Test-Path $stateFile) {
        $state = Get-Content $stateFile | ConvertFrom-Json

        foreach ($processId in @($state.ExternalBootstrapPid, $state.BrokerPid)) {
            if ($processId) {
                Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
            }
        }

        Remove-Item $stateFile -Force -ErrorAction SilentlyContinue
    }

    foreach ($port in 32092, 32090) {
        $connections = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue

        foreach ($connection in $connections) {
            $process = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
            if ($process -and $process.ProcessName -eq 'kubectl') {
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }
        }
    }
}

function Wait-ForDeployment {
    param(
        [string]$Namespace,
        [string]$Name,
        [int]$TimeoutSeconds = 300
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    do {
        $deployment = kubectl get deployment $Name -n $Namespace --ignore-not-found
        if ($deployment) {
            kubectl rollout status deployment/$Name -n $Namespace --timeout="$($TimeoutSeconds)s"
            return
        }

        Start-Sleep -Seconds 5
    } while ((Get-Date) -lt $deadline)

    throw "Deployment '$Name' não apareceu no namespace '$Namespace' dentro do tempo esperado."
}

function Start-PortForward {
    param(
        [string]$Command,
        [int]$LocalPort
    )

    $process = Start-Process powershell `
        -WindowStyle Hidden `
        -PassThru `
        -ArgumentList '-NoProfile', '-ExecutionPolicy', 'Bypass', '-Command', $Command

    $deadline = (Get-Date).AddSeconds(20)

    do {
        $connection = Get-NetTCPConnection -LocalPort $LocalPort -State Listen -ErrorAction SilentlyContinue
        if ($connection) {
            return $process
        }

        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline -and -not $process.HasExited)

    throw "Não foi possível expor a porta local $LocalPort com kubectl port-forward."
}

function Wait-ForApiHealth {
    param(
        [string]$Url,
        [int]$TimeoutSeconds = 120
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    do {
        try {
            $response = Invoke-WebRequest -UseBasicParsing $Url -TimeoutSec 10
            if ($response.StatusCode -eq 200) {
                return $response
            }
        }
        catch {
            Start-Sleep -Seconds 3
        }
    } while ((Get-Date) -lt $deadline)

    throw "A API não respondeu com sucesso em '$Url' dentro do tempo esperado."
}

New-Item -ItemType Directory -Path $stateDir -Force | Out-Null
Set-Location $repoRoot

Write-Step 'Validando Kubernetes local'
kubectl config current-context
kubectl get nodes

Write-Step 'Garantindo namespace kafka'
kubectl create namespace kafka --dry-run=client -o yaml | kubectl apply -f -

Write-Step 'Instalando ou atualizando Strimzi Operator'
kubectl apply -f "https://strimzi.io/install/latest?namespace=kafka" -n kafka
Wait-ForDeployment -Namespace 'kafka' -Name 'strimzi-cluster-operator'

Write-Step 'Aplicando cluster Kafka e topico'
kubectl apply -f deploy/strimzi/kafkanodepool-my-cluster.yaml
kubectl apply -f deploy/strimzi/kafka-my-cluster.yaml
kubectl wait kafka/my-cluster --for=condition=Ready --timeout=600s -n kafka
kubectl apply -f deploy/strimzi/kafkatopic-pedido.yaml
kubectl wait kafkatopic/pedido --for=condition=Ready --timeout=300s -n kafka

Write-Step 'Reiniciando port-forwards locais do Strimzi'
Stop-PortForwards

$externalBootstrap = Start-PortForward -Command 'kubectl port-forward svc/my-cluster-kafka-external-bootstrap 32092:9094 -n kafka' -LocalPort 32092
$broker = Start-PortForward -Command 'kubectl port-forward svc/my-cluster-dual-role-0 32090:9094 -n kafka' -LocalPort 32090

[pscustomobject]@{
    ExternalBootstrapPid = $externalBootstrap.Id
    BrokerPid = $broker.Id
} | ConvertTo-Json | Set-Content -Path $stateFile

Write-Step 'Subindo API, SQL Server e Worker'
if ($SkipBuild) {
    docker compose -f docker-compose.yml -f docker-compose.kafka.yml up -d
}
else {
    docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
}

Write-Step 'Validando status dos containers'
docker compose -f docker-compose.yml -f docker-compose.kafka.yml ps

Write-Step 'Validando health da API'
$health = Wait-ForApiHealth -Url 'http://localhost:8080/health'
Write-Host "Health check: $($health.StatusCode)" -ForegroundColor Green

Write-Step 'Ambiente pronto'
Write-Host 'Swagger: http://localhost:8080/swagger'
