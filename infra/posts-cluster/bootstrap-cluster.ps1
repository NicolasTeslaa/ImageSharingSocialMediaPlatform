$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$composeFile = Join-Path $scriptRoot "docker-compose.yml"
$clusterScript = Join-Path $scriptRoot "scripts\setupCluster.js"

if (-not (Get-Command mysqlsh -ErrorAction SilentlyContinue)) {
    throw "mysqlsh nao encontrado. Instale o MySQL Shell e execute novamente."
}

docker compose -f $composeFile up -d posts-db-1 posts-db-2 posts-db-3

mysqlsh --js --file $clusterScript

docker compose -f $composeFile up -d posts-db-router

Write-Host "Cluster do PostsService configurado. Writer: localhost:6446 | Reader: localhost:6447"
