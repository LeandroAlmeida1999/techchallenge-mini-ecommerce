param(
    [switch]$RemoveKafkaResources,
    [switch]$RemoveNamespace
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$stateFile = Join-Path (Join-Path $PSScriptRoot '.state') 'port-forwards.json'

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

Set-Location $repoRoot

Write-Step 'Parando docker compose'
docker compose -f docker-compose.yml -f docker-compose.kafka.yml down

Write-Step 'Encerrando port-forwards'
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

if ($RemoveKafkaResources) {
    Write-Step 'Removendo topico e cluster Kafka local'
    kubectl delete -f deploy/strimzi/kafkatopic-pedido.yaml --ignore-not-found=true
    kubectl delete -f deploy/strimzi/kafka-my-cluster.yaml --ignore-not-found=true
    kubectl delete -f deploy/strimzi/kafkanodepool-my-cluster.yaml --ignore-not-found=true
}

if ($RemoveNamespace) {
    Write-Step 'Removendo namespace kafka'
    kubectl delete namespace kafka --ignore-not-found=true
}

Write-Step 'Ambiente encerrado'
