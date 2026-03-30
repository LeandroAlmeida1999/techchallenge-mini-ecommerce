param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

Write-Host ""
Write-Host "==> Subindo aplicação com docker compose" -ForegroundColor Cyan

if ($SkipBuild) {
    docker compose -f docker-compose.yml -f docker-compose.kafka.yml up -d
}
else {
    docker compose -f docker-compose.yml -f docker-compose.kafka.yml up --build -d
}

Write-Host ""
Write-Host "==> Status dos serviços" -ForegroundColor Cyan
docker compose -f docker-compose.yml -f docker-compose.kafka.yml ps

Write-Host ""
Write-Host "Swagger: http://localhost:8080/swagger" -ForegroundColor Green

