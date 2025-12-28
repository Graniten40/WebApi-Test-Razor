USE `sql-music`;

/* Remove stored procedures */
DROP PROCEDURE IF EXISTS `sql-music`.`gstusr_spLogin`;
DROP PROCEDURE IF EXISTS `sql-music`.`supusr_spDeleteAll`;

/* Remove roles */
DROP ROLE IF EXISTS 'gstUsrRole';
DROP ROLE IF EXISTS 'usrRole';
DROP ROLE IF EXISTS 'supUsrRole';
DROP ROLE IF EXISTS 'dboRole';

/* Remove users */
DROP USER IF EXISTS 'gstusr'@'%';
DROP USER IF EXISTS 'usr'@'%';
DROP USER IF EXISTS 'supusr'@'%';
DROP USER IF EXISTS 'dbo'@'%';

/* Flush privileges after user/role changes */
FLUSH PRIVILEGES;

/* Remove views */
DROP VIEW IF EXISTS `sql-music`.`gstusr_vwInfoDb`;

/* Drop tables in the right order to avoid FK conflicts */
DROP TABLE IF EXISTS `sql-music`.`supusr_ArtistDbMMusicGroupDbM`;
DROP TABLE IF EXISTS `sql-music`.`supusr_Artists`;
DROP TABLE IF EXISTS `sql-music`.`supusr_Albums`;
DROP TABLE IF EXISTS `sql-music`.`supusr_MusicGroups`;
DROP TABLE IF EXISTS `sql-music`.`dbo_Users`;
DROP TABLE IF EXISTS `sql-music`.`__EFMigrationsHistory`;

