USE `sql-music`;


/* MariaDB does not support CREATE SCHEMA as a namespace, but as a synonym for CREATE DATABASE. */
/* If you want to use schemas as in SQL Server, use database prefixes or just ignore this step if all tables are in the same database. */

/* Create a views */
CREATE OR REPLACE VIEW gstusr_vwInfoDb AS
    SELECT (SELECT COUNT(*) FROM supusr_MusicGroups WHERE Seeded = 1) as NrSeededMusicGroups, 
        (SELECT COUNT(*) FROM supusr_MusicGroups WHERE Seeded = 0) as NrUnseededMusicGroups,
        (SELECT COUNT(*) FROM supusr_Albums WHERE Seeded = 1) as NrSeededAlbums, 
        (SELECT COUNT(*) FROM supusr_Albums WHERE Seeded = 0) as NrUnseededAlbums,
        (SELECT COUNT(*) FROM supusr_Artists WHERE Seeded = 1) as NrSeededArtists, 
        (SELECT COUNT(*) FROM supusr_Artists WHERE Seeded = 0) as NrUnseededArtists;

DELIMITER $$

/* Create the DeleteAll procedure */
CREATE OR REPLACE DEFINER='dbo'@'%' PROCEDURE supusr_spDeleteAll(
    IN seededParam BOOLEAN,
    OUT nrMusicGroupsAffected INT,
    OUT nrAlbumsAffected INT,
    OUT nrArtistsAffected INT
)
BEGIN
    SELECT  COUNT(*) INTO nrMusicGroupsAffected FROM supusr_MusicGroups WHERE Seeded = seededParam;
    SELECT  COUNT(*) INTO nrAlbumsAffected FROM supusr_Albums WHERE Seeded = seededParam;
    SELECT  COUNT(*) INTO nrArtistsAffected FROM supusr_Artists WHERE Seeded = seededParam;
    
    DELETE FROM supusr_MusicGroups WHERE Seeded = seededParam;
    DELETE FROM supusr_Albums WHERE Seeded = seededParam;
    DELETE FROM supusr_Artists WHERE Seeded = seededParam;
    
    /* test to throw an error */
    /* SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error occurred in supusr_spDeleteAll'; */

    SELECT * FROM gstusr_vwInfoDb;
END$$

DELIMITER ;

/* User and role management in MariaDB */
/* Create users and logins if they do not exist */
CREATE USER IF NOT EXISTS 'gstusr'@'%' IDENTIFIED BY 'pa$Word1';
CREATE USER IF NOT EXISTS 'usr'@'%' IDENTIFIED BY 'pa$Word1';
CREATE USER IF NOT EXISTS 'supusr'@'%' IDENTIFIED BY 'pa$Word1';
CREATE USER IF NOT EXISTS 'dbo'@'%' IDENTIFIED BY 'pa$Word1';

/* Grant database access privileges */
GRANT USAGE ON `sql-music`.* TO 'gstusr'@'%';
GRANT USAGE ON `sql-music`.* TO 'usr'@'%';
GRANT USAGE ON `sql-music`.* TO 'supusr'@'%';
GRANT ALL PRIVILEGES ON `sql-music`.* TO 'dbo'@'%';

/* Create roles */
CREATE ROLE IF NOT EXISTS 'gstUsrRole';
CREATE ROLE IF NOT EXISTS 'usrRole';
CREATE ROLE IF NOT EXISTS 'supUsrRole';
CREATE ROLE IF NOT EXISTS 'dboRole';

/* Grant role privileges (adjust as needed) */
GRANT SELECT ON `sql-music`.`gstusr_vwInfoDb` TO 'gstUsrRole';
GRANT EXECUTE ON PROCEDURE `sql-music`.`gstusr_spLogin` TO 'gstUsrRole';

GRANT 'gstUsrRole' TO 'usrRole'; /* usr is also a gstusr */
GRANT SELECT, UPDATE, INSERT ON `sql-music`.`supusr_Albums` TO 'usrRole';
GRANT SELECT, UPDATE, INSERT ON `sql-music`.`supusr_ArtistDbMMusicGroupDbM` TO 'usrRole';
GRANT SELECT, UPDATE, INSERT ON `sql-music`.`supusr_MusicGroups` TO 'usrRole';
GRANT SELECT, UPDATE, INSERT ON `sql-music`.`supusr_Artists` TO 'usrRole';

GRANT 'usrRole' TO 'supUsrRole'; /* supusr is also a usr */
GRANT DELETE ON `sql-music`.`supusr_Albums` TO 'supUsrRole';
GRANT DELETE ON `sql-music`.`supusr_ArtistDbMMusicGroupDbM` TO 'supUsrRole';
GRANT DELETE ON `sql-music`.`supusr_MusicGroups` TO 'supUsrRole';
GRANT DELETE ON `sql-music`.`supusr_Artists` TO 'supUsrRole';
GRANT EXECUTE ON PROCEDURE `sql-music`.`supusr_spDeleteAll` TO 'supUsrRole';

/* Grant role privileges for dboRole (full privileges) */
GRANT ALL PRIVILEGES ON `sql-music`.* TO 'dboRole';

/* Assign users to Roles */
GRANT 'gstUsrRole' TO 'gstusr'@'%';
GRANT 'usrRole' TO 'usr'@'%';
GRANT 'supUsrRole' TO 'supusr'@'%';
GRANT 'dboRole' TO 'dbo'@'%';

/* Set default roles for users */
SET DEFAULT ROLE gstUsrRole FOR 'gstusr'@'%';
SET DEFAULT ROLE usrRole FOR 'usr'@'%';
SET DEFAULT ROLE supUsrRole FOR 'supusr'@'%';
SET DEFAULT ROLE dboRole FOR 'dbo'@'%';

/* Flush privileges to ensure changes take effect */
FLUSH PRIVILEGES;
