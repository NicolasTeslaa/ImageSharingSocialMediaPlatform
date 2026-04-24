$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$composeFile = Join-Path $scriptRoot "docker-compose.yml"
$clusterScript = Join-Path $scriptRoot "scripts\setupCluster.js"

$hasLocalMysqlShell = $null -ne (Get-Command mysqlsh -ErrorAction SilentlyContinue)
$rootPassword = "root123!"
$appUser = "posts_app"
$appPassword = "posts_app_123"

docker compose -f $composeFile up -d posts-db-1 posts-db-2 posts-db-3

if ($hasLocalMysqlShell) {
    mysqlsh --js --file $clusterScript
}
else {
    Write-Host "mysqlsh nao encontrado localmente. Aplicando bootstrap SQL simplificado no posts-db-1..."

    $sql = @"
CREATE DATABASE IF NOT EXISTS ImageSharingPostsDb;
CREATE DATABASE IF NOT EXISTS ImageSharingTimelineDb;
CREATE DATABASE IF NOT EXISTS ImageSharingUsersDb;
CREATE USER IF NOT EXISTS '$appUser'@'%' IDENTIFIED BY '$appPassword';
GRANT ALL PRIVILEGES ON ImageSharingPostsDb.* TO '$appUser'@'%';
GRANT ALL PRIVILEGES ON ImageSharingTimelineDb.* TO '$appUser'@'%';
GRANT ALL PRIVILEGES ON ImageSharingUsersDb.* TO '$appUser'@'%';
FLUSH PRIVILEGES;
"@

    $sqlOneLine = ($sql -replace "`r", " " -replace "`n", " ").Trim()
    docker exec posts-db-1 sh -c "mysql -uroot -p$rootPassword -e \"$sqlOneLine\""
}

docker compose -f $composeFile up -d posts-db-router posts-kafka posts-kafka-ui posts-elasticsearch posts-minio posts-minio-init

function Wait-ForTcpPort {
    param (
        [string]$HostName,
        [int]$Port,
        [int]$MaxAttempts = 60,
        [int]$DelaySeconds = 2
    )

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $result = Test-NetConnection -ComputerName $HostName -Port $Port -WarningAction SilentlyContinue
        if ($result.TcpTestSucceeded) {
            return
        }

        Write-Host "Aguardando $HostName`:$Port ($attempt/$MaxAttempts)..."
        Start-Sleep -Seconds $DelaySeconds
    }

    throw "Timeout aguardando porta $HostName`:$Port."
}

Wait-ForTcpPort -HostName "localhost" -Port 33061
Wait-ForTcpPort -HostName "localhost" -Port 9200
Wait-ForTcpPort -HostName "localhost" -Port 9092
Wait-ForTcpPort -HostName "localhost" -Port 9000

Write-Host "Infra configurada. MySQL (33061), Elasticsearch (9200), Kafka (9092) e MinIO (9000/9001) prontos."
