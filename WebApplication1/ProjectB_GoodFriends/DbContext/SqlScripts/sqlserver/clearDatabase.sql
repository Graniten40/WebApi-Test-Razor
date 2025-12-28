USE [sql-friends];
--GO

-- remove stored procedures
DROP PROCEDURE IF EXISTS gstusr.spLogin
DROP PROCEDURE IF EXISTS supusr.spDeleteAll
GO

-- remove roles
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'gstUsrRole' AND type = 'R')
BEGIN
    ALTER ROLE gstUsrRole DROP MEMBER gstusrUser;
    ALTER ROLE gstUsrRole DROP MEMBER usrUser;
    ALTER ROLE gstUsrRole DROP MEMBER supusrUser;
END

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'usrRole' AND type = 'R')
BEGIN
    ALTER ROLE usrRole DROP MEMBER usrUser;
    ALTER ROLE usrRole DROP MEMBER supusrUser;
END

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'supUsrRole' AND type = 'R')
BEGIN
    ALTER ROLE supUsrRole DROP MEMBER supusrUser;
END

-- Remove dbo user from db_owner role
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'dboUser' AND type = 'S')
BEGIN
    ALTER ROLE db_owner DROP MEMBER dboUser;
END

-- remove roles
DROP ROLE IF EXISTS gstUsrRole;
DROP ROLE IF EXISTS usrRole;
DROP ROLE IF EXISTS supUsrRole;
GO

--remove users
DROP USER IF EXISTS gstusrUser;
DROP USER IF EXISTS usrUser;
DROP USER IF EXISTS supusrUser;
DROP USER IF EXISTS dboUser;
GO

-- remove logins
IF SUSER_ID (N'gstusr') IS NOT NULL
DROP LOGIN gstusr;

IF SUSER_ID (N'usr') IS NOT NULL
DROP LOGIN usr;

IF SUSER_ID (N'supusr') IS NOT NULL
DROP LOGIN supusr;

IF SUSER_ID (N'dbo') IS NOT NULL
DROP LOGIN dbo;

-- remove views
DROP VIEW IF EXISTS [gstusr].[vwInfoDb]
DROP VIEW IF EXISTS [gstusr].[vwInfoFriends]
DROP VIEW IF EXISTS [gstusr].[vwInfoPets]
DROP VIEW IF EXISTS [gstusr].[vwInfoQuotes]
GO
    
-- Drop tables in the right order to avoid FK conflicts
DROP TABLE IF EXISTS supusr.FriendDbMQuoteDbM;
DROP TABLE IF EXISTS supusr.Pets;
DROP TABLE IF EXISTS supusr.Quotes;
DROP TABLE IF EXISTS supusr.Friends;
DROP TABLE IF EXISTS supusr.Addresses;
DROP TABLE IF EXISTS dbo.Users;
DROP TABLE IF EXISTS __EFMigrationsHistory;


-- drop schemas
DROP SCHEMA IF EXISTS gstusr;
DROP SCHEMA IF EXISTS usr;
DROP SCHEMA IF EXISTS supusr;
GO