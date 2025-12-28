USE [sql-music];
GO

--create a schemas
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'gstusr')
    EXEC('CREATE SCHEMA gstusr');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'usr')
    EXEC('CREATE SCHEMA usr');
GO

--create a view that gives overview of the database content
CREATE OR ALTER VIEW gstusr.vwInfoDb AS
    SELECT (SELECT COUNT(*) FROM supusr.MusicGroups WHERE Seeded = 1) as nrSeededMusicGroups, 
        (SELECT COUNT(*) FROM supusr.MusicGroups WHERE Seeded = 0) as nrUnseededMusicGroups,
        (SELECT COUNT(*) FROM supusr.Albums WHERE Seeded = 1) as nrSeededAlbums, 
        (SELECT COUNT(*) FROM supusr.Albums WHERE Seeded = 0) as nrUnseededAlbums,
        (SELECT COUNT(*) FROM supusr.Artists WHERE Seeded = 1) as nrSeededArtists, 
        (SELECT COUNT(*) FROM supusr.Artists WHERE Seeded = 0) as NrUnseededArtists;
GO

--create the DeleteAll procedure
CREATE OR ALTER PROC supusr.spDeleteAll
    @seededParam BIT = 1,
    @nrMusicGroupsAffected INT OUTPUT,
    @nrAlbumsAffected INT OUTPUT,
    @nrArtistsAffected INT OUTPUT
AS

    SET NOCOUNT ON;

    SELECT  @nrMusicGroupsAffected = COUNT(*) FROM supusr.MusicGroups WHERE Seeded = @seededParam;
    SELECT  @nrAlbumsAffected = COUNT(*) FROM supusr.Albums WHERE Seeded = @seededParam;
    SELECT  @nrArtistsAffected = COUNT(*) FROM supusr.Artists WHERE Seeded = @seededParam;

    DELETE FROM supusr.MusicGroups WHERE Seeded = @seededParam;
    DELETE FROM supusr.Albums WHERE Seeded = @seededParam;
    DELETE FROM supusr.Artists WHERE Seeded = @seededParam;

    --throw our own error
    --;THROW 999999, 'Error occurred in supusr.spDeleteAll', 1

    SELECT * FROM gstusr.vwInfoDb;
GO

-- User and role management in sqlserver
-- Create logins
IF SUSER_ID (N'gstusr') IS NULL
    CREATE LOGIN gstusr WITH PASSWORD=N'pa$Word1', 
        DEFAULT_DATABASE=[sql-music], DEFAULT_LANGUAGE=us_english, 
        CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

IF SUSER_ID (N'usr') IS NULL
    CREATE LOGIN usr WITH PASSWORD=N'pa$Word1', 
        DEFAULT_DATABASE=[sql-music], DEFAULT_LANGUAGE=us_english, 
        CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

IF SUSER_ID (N'supusr') IS NULL
    CREATE LOGIN supusr WITH PASSWORD=N'pa$Word1', 
        DEFAULT_DATABASE=[sql-music], DEFAULT_LANGUAGE=us_english, 
        CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

IF SUSER_ID (N'dbo') IS NULL
    CREATE LOGIN dbo WITH PASSWORD=N'pa$Word1', 
        DEFAULT_DATABASE=[sql-music], DEFAULT_LANGUAGE=us_english, 
        CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;


--create users
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'gstusrUser' AND type = 'S')
    CREATE USER gstusrUser FROM LOGIN gstusr;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'usrUser' AND type = 'S')
    CREATE USER usrUser FROM LOGIN usr;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'supusrUser' AND type = 'S')
    CREATE USER supusrUser FROM LOGIN supusr;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'dboUser' AND type = 'S')
    CREATE USER dboUser FROM LOGIN dbo;

--create roles
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'gstUsrRole' AND type = 'R')
    CREATE ROLE gstUsrRole;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'usrRole' AND type = 'R')
    CREATE ROLE usrRole;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'supUsrRole' AND type = 'R')
    CREATE ROLE supUsrRole;

-- Grant role privileges (adjust as needed)
GRANT SELECT, EXECUTE ON SCHEMA::gstusr to gstUsrRole;
GRANT SELECT, UPDATE, INSERT ON SCHEMA::supusr to usrRole;
GRANT SELECT, UPDATE, INSERT, DELETE, EXECUTE ON SCHEMA::supusr to supUsrRole;

-- Grant full privileges to dboRole using built-in db_owner role
ALTER ROLE db_owner ADD MEMBER dboUser;

-- Assign users to Roles
ALTER ROLE gstUsrRole ADD MEMBER gstusrUser;

ALTER ROLE gstUsrRole ADD MEMBER usrUser;
ALTER ROLE usrRole ADD MEMBER usrUser;

ALTER ROLE gstUsrRole ADD MEMBER supusrUser;
ALTER ROLE usrRole ADD MEMBER supusrUser;
ALTER ROLE supUsrRole ADD MEMBER supusrUser;
GO




