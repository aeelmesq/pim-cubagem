#!/usr/bin/env bash
# Cria no SQL Server deste PC o usuário que a API da VPS usa (em vez do sa).
# Pode rodar de novo: se o usuário existir, só troca a senha.
set -euo pipefail
cd "$(dirname "$0")/.."
source deploy/.env

docker compose up -d sqlserver
until [ "$(docker inspect -f '{{.State.Health.Status}}' cubagemcaminhao-sql)" = healthy ]; do sleep 3; done

# A senha vai por variável de ambiente, para não aparecer na linha de comando.
docker exec -i -e DB_USER="$DB_USER" -e DB_PASSWORD="$DB_PASSWORD" cubagemcaminhao-sql bash -c \
  '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b' <<'SQL'
IF DB_ID('CubagemCaminhaoDb') IS NULL CREATE DATABASE CubagemCaminhaoDb;
GO
IF SUSER_ID('$(DB_USER)') IS NULL
    CREATE LOGIN [$(DB_USER)] WITH PASSWORD = '$(DB_PASSWORD)', CHECK_POLICY = ON;
ELSE
    ALTER LOGIN [$(DB_USER)] WITH PASSWORD = '$(DB_PASSWORD)';
GO
USE CubagemCaminhaoDb;
IF USER_ID('$(DB_USER)') IS NULL CREATE USER [$(DB_USER)] FOR LOGIN [$(DB_USER)];
ALTER ROLE db_owner ADD MEMBER [$(DB_USER)];
GO
SQL
echo "Usuário $DB_USER pronto no banco CubagemCaminhaoDb."
