IF DB_ID(N'CubagemCaminhaoDb') IS NULL
BEGIN
    CREATE DATABASE CubagemCaminhaoDb;
END;
GO

USE CubagemCaminhaoDb;
GO

-- Estrutura inicial do banco para a próxima etapa do projeto.
-- A modelagem detalhada será implementada no próximo passo.
IF OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaVersion (
        Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        VersionNumber INT NOT NULL,
        AppliedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END;
GO
