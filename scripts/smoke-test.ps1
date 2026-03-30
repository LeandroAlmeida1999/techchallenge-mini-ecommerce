param(
    [string]$BaseUrl = 'http://localhost:8080',
    [switch]$ConsumeKafka
)

$ErrorActionPreference = 'Stop'

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Url,
        [object]$Body
    )

    if ($null -eq $Body) {
        return Invoke-RestMethod -Method $Method -Uri $Url -TimeoutSec 60
    }

    return Invoke-RestMethod -Method $Method -Uri $Url -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Compress) -TimeoutSec 60
}

function Get-OutboxStatus {
    docker compose -f docker-compose.yml -f docker-compose.kafka.yml exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_strong_password123" -C -d ECommerceDb -Q "SET NOCOUNT ON; SELECT [Status], COUNT(*) AS [Count] FROM OutboxMessages GROUP BY [Status] ORDER BY [Status];"
}

function Get-LatestOutboxMessage {
    docker compose -f docker-compose.yml -f docker-compose.kafka.yml exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Your_strong_password123" -C -d ECommerceDb -Q "SET NOCOUNT ON; SELECT TOP 1 [Status], [Retries], [ProcessedOnUtc] FROM OutboxMessages ORDER BY [OccurredOnUtc] DESC;"
}

Write-Step 'Criando cliente'
$cliente = Invoke-Api -Method 'POST' -Url "$BaseUrl/clientes" -Body @{
    nome = 'Cliente Smoke Test'
    email = "smoke.$([guid]::NewGuid().ToString('N'))@email.com"
}

Write-Step 'Criando produto'
$produto = Invoke-Api -Method 'POST' -Url "$BaseUrl/produtos" -Body @{
    nome = 'Produto Smoke Test'
    preco = 99.90
    ativo = $true
}

Write-Step 'Criando e confirmando pedido'
$pedido = Invoke-Api -Method 'POST' -Url "$BaseUrl/pedidos" -Body @{
    clienteId = $cliente.id
}

Invoke-Api -Method 'POST' -Url "$BaseUrl/pedidos/$($pedido.id)/itens" -Body @{
    produtoId = $produto.id
    quantidade = 1
} | Out-Null

Invoke-Api -Method 'POST' -Url "$BaseUrl/pedidos/$($pedido.id)/confirmar" -Body $null | Out-Null

Write-Step 'Consultando pedido final'
$pedidoFinal = Invoke-Api -Method 'GET' -Url "$BaseUrl/pedidos/$($pedido.id)" -Body $null

Write-Step 'Consultando status do outbox'
$latestOutbox = $null
$outbox = $null

for ($attempt = 1; $attempt -le 6; $attempt++) {
    $latestOutbox = Get-LatestOutboxMessage
    $outbox = Get-OutboxStatus
    if ($latestOutbox -match 'Processed') {
        break
    }

    Start-Sleep -Seconds 5
}

if ($ConsumeKafka) {
    Write-Step 'Consumindo uma mensagem do topico pedido'
    kubectl delete pod kafka-consumer-check -n kafka --ignore-not-found=true | Out-Null

    $stdoutFile = [System.IO.Path]::GetTempFileName()
    $stderrFile = [System.IO.Path]::GetTempFileName()
    $separator = '__KAFKA_KEY_SEPARATOR__'

    try {
        Start-Process `
            -FilePath 'kubectl' `
            -ArgumentList @(
                'run',
                'kafka-consumer-check',
                '--image=quay.io/strimzi/kafka:0.51.0-kafka-4.2.0',
                '-n', 'kafka',
                '--rm',
                '-i',
                '--restart=Never',
                '--command',
                '--',
                'bin/kafka-console-consumer.sh',
                '--bootstrap-server', 'my-cluster-kafka-bootstrap:9092',
                '--topic', 'pedido',
                '--from-beginning',
                '--timeout-ms', '10000',
                '--formatter-property', 'print.key=true',
                '--formatter-property', "key.separator=$separator"
            ) `
            -Wait `
            -NoNewWindow `
            -RedirectStandardOutput $stdoutFile `
            -RedirectStandardError $stderrFile | Out-Null

        $consumerOutput = Get-Content $stdoutFile -ErrorAction SilentlyContinue
    }
    finally {
        Remove-Item $stdoutFile, $stderrFile -Force -ErrorAction SilentlyContinue
    }

    $matchingMessage = $consumerOutput | Where-Object { $_ -match [regex]::Escape($pedidoFinal.id) } | Select-Object -First 1

    if ($matchingMessage) {
        $parts = $matchingMessage -split [regex]::Escape($separator), 2
        $messageKey = if ($parts.Count -ge 1) { $parts[0].Trim() } else { '' }
        $messagePayload = if ($parts.Count -eq 2) { $parts[1].Trim() } else { $matchingMessage.Trim() }

        Write-Host ""
        Write-Host "Mensagem encontrada no Kafka" -ForegroundColor Green
        Write-Host "Chave  : $messageKey"
        Write-Host "Payload: $messagePayload"
    }
    else {
        Write-Host "Nenhuma mensagem do PedidoId $($pedidoFinal.id) foi encontrada na leitura do tópico." -ForegroundColor Yellow
    }
}
else {
    Write-Step 'Consumo Kafka nao solicitado'
    Write-Host "Execute o mesmo script com -ConsumeKafka para ler uma mensagem do topico 'pedido'." -ForegroundColor Yellow
}

Write-Step 'Resumo'
[pscustomobject]@{
    ClienteId = $cliente.id
    ProdutoId = $produto.id
    PedidoId = $pedidoFinal.id
    Status = $pedidoFinal.status
    ValorTotal = $pedidoFinal.valorTotal
} | Format-List

Write-Host ""
Write-Host $latestOutbox
Write-Host ""
Write-Host $outbox
