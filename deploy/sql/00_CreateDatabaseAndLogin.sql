-- One-time setup for hosting DJ MrKos on a SQL Server instance shared with other apps.
-- Run as sa (or another sysadmin). Creates the database and a dedicated login that can
-- only touch djmrkos — the other databases on the instance stay out of its reach.
--
-- Replace CAMBIA_ESTA_CONTRASEÑA before running. The schema itself is NOT created here:
-- run DjMrkos.Migrator afterwards with the djmrkos_app login (see deploy/README.md), so
-- DbUp's SchemaVersions table records which scripts were applied.

IF DB_ID(N'djmrkos') IS NULL
    CREATE DATABASE djmrkos;
GO

IF SUSER_ID(N'djmrkos_app') IS NULL
    CREATE LOGIN djmrkos_app
        WITH PASSWORD = N'CAMBIA_ESTA_CONTRASEÑA',
             DEFAULT_DATABASE = djmrkos,
             CHECK_POLICY = ON;
GO

USE djmrkos;
GO

IF USER_ID(N'djmrkos_app') IS NULL
    CREATE USER djmrkos_app FOR LOGIN djmrkos_app;
GO

-- db_owner is needed because the migrator runs DDL (CREATE TABLE, ALTER TABLE...).
ALTER ROLE db_owner ADD MEMBER djmrkos_app;
GO
