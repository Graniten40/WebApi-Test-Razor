USE `sql-friends`;

/* Remove stored procedures */
DROP PROCEDURE IF EXISTS `sql-friends`.`gstusr_spLogin`;
DROP PROCEDURE IF EXISTS `sql-friends`.`supusr_spDeleteAll`;

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
DROP VIEW IF EXISTS `sql-friends`.`gstusr_vwInfoDb`;
DROP VIEW IF EXISTS `sql-friends`.`gstusr_vwInfoFriends`;
DROP VIEW IF EXISTS `sql-friends`.`gstusr_vwInfoPets`;
DROP VIEW IF EXISTS `sql-friends`.`gstusr_vwInfoQuotes`;

/* Drop tables in the right order to avoid FK conflicts */
DROP TABLE IF EXISTS `sql-friends`.`supusr_FriendDbMQuoteDbM`;
DROP TABLE IF EXISTS `sql-friends`.`supusr_Pets`;
DROP TABLE IF EXISTS `sql-friends`.`supusr_Quotes`;
DROP TABLE IF EXISTS `sql-friends`.`supusr_Friends`;
DROP TABLE IF EXISTS `sql-friends`.`supusr_Addresses`;
DROP TABLE IF EXISTS `sql-friends`.`dbo_Users`;
DROP TABLE IF EXISTS `sql-friends`.`__EFMigrationsHistory`;

