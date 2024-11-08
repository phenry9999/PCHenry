declare @backup int = 1
	declare @backupDescription nvarchar(1000) = 'Back to passing integration tests'
declare @restore int = 0
	declare @backupPositionToRestore int = 999
 
/*
--Look at the backups to find out which file position to restore
DECLARE @BackupHistory TABLE(BackupName nvarchar(128),BackupDescription nvarchar(255),BackupType smallint,ExpirationDate datetime,Compressed bit,Position smallint,DeviceType tinyint, UserName nvarchar(128),ServerName nvarchar(128),DatabaseName nvarchar(128),DatabaseVersion int,DatabaseCreationDate datetime,BackupSize numeric(20, 0),FirstLSN numeric(25, 0),LastLSN numeric(25, 0),CheckpointLSN numeric(25, 0),DatabaseBackupLSN numeric(25, 0),BackupStartDate datetime,BackupFinishDate datetime,SortOrder smallint,[CodePage] smallint,UnicodeLocaleId int,UnicodeComparisonStyle int,CompatibilityLevel tinyint,SoftwareVendorId int,SoftwareVersionMajor int,SoftwareVersionMinor int,SoftwareVersionBuild int,MachineName nvarchar(128),Flags int,BindingId uniqueidentifier,RecoveryForkId uniqueidentifier,Collation nvarchar(128),FamilyGUID uniqueidentifier,HasBulkLoggedData bit,IsSnapshot bit,IsReadOnly bit,IsSingleUser bit,HasBackupChecksums bit,IsDamaged bit,BeginsLogChain bit,HasIncompleteMetaData bit,IsForceOffline bit,IsCopyOnly bit,FirstRecoveryForkID uniqueidentifier,ForkPointLSN numeric(25, 0),RecoveryModel nvarchar(60),DifferentialBaseLSN numeric(25, 0),DifferentialBaseGUID uniqueidentifier,BackupTypeDescription nvarchar(60),BackupSetGUID uniqueidentifier,CompressedBackupSize bigint,Containment tinyint,KeyAlgorithm nvarchar(32),EncryptorThumbprint varbinary(20),EncryptorType nvarchar(32))
INSERT INTO @BackupHistory EXEC ('RESTORE HEADERONLY FROM DISK = N''Z:\DB_Backup\AvantiAtlasSuite\AtlasIntegrationTests.bak''')
SELECT Position, BackupName, BackupDescription,DatabaseName, BackupStartDate, BackupType FROM @BackupHistory ORDER BY BackupStartDate DESC
delete from @BackupHistory
INSERT INTO @BackupHistory EXEC ('RESTORE HEADERONLY FROM DISK = N''Z:\DB_Backup\AvantiAtlasSuite\AtlasIntegrationTestsNET.bak''')
SELECT Position, BackupName, BackupDescription,DatabaseName, BackupStartDate, BackupType FROM @BackupHistory ORDER BY BackupStartDate DESC
*/
 
if @backup = 1 
begin
	print 'Starting Backup'
	use AtlasIntegrationTests
 
	print 'Backing up AtlasIntegrationTests'
	BACKUP DATABASE AtlasIntegrationTests TO  DISK = N'Z:\DB_Backup\AvantiAtlasSuite\AtlasIntegrationTests.bak' WITH NOFORMAT, NOINIT,  NAME = N'AtlasIntegrationTests-Full Database Backup', Description = @backupDescription, SKIP, NOREWIND, NOUNLOAD,  STATS = 10
 
	print 'Backing up AtlasIntegrationTestsNet'
	BACKUP DATABASE AtlasIntegrationTestsNet TO  DISK = N'Z:\DB_Backup\AvantiAtlasSuite\AtlasIntegrationTestsNET.bak' WITH NOFORMAT, NOINIT,  NAME = N'AtlasIntegrationTestsNET-Full Database Backup', Description = @backupDescription, SKIP, NOREWIND, NOUNLOAD,  STATS = 10
	print 'Finished Backup'
end
 
if @restore = 1 
begin
	print 'Starting Restore'
	USE master
 
	DECLARE @BackupHistory TABLE(BackupName nvarchar(128),BackupDescription nvarchar(255),BackupType smallint,ExpirationDate datetime,Compressed bit,Position smallint,DeviceType tinyint, UserName nvarchar(128),ServerName nvarchar(128),DatabaseName nvarchar(128),DatabaseVersion int,DatabaseCreationDate datetime,BackupSize numeric(20, 0),FirstLSN numeric(25, 0),LastLSN numeric(25, 0),CheckpointLSN numeric(25, 0),DatabaseBackupLSN numeric(25, 0),BackupStartDate datetime,BackupFinishDate datetime,SortOrder smallint,[CodePage] smallint,UnicodeLocaleId int,UnicodeComparisonStyle int,CompatibilityLevel tinyint,SoftwareVendorId int,SoftwareVersionMajor int,SoftwareVersionMinor int,SoftwareVersionBuild int,MachineName nvarchar(128),Flags int,BindingId uniqueidentifier,RecoveryForkId uniqueidentifier,Collation nvarchar(128),FamilyGUID uniqueidentifier,HasBulkLoggedData bit,IsSnapshot bit,IsReadOnly bit,IsSingleUser bit,HasBackupChecksums bit,IsDamaged bit,BeginsLogChain bit,HasIncompleteMetaData bit,IsForceOffline bit,IsCopyOnly bit,FirstRecoveryForkID uniqueidentifier,ForkPointLSN numeric(25, 0),RecoveryModel nvarchar(60),DifferentialBaseLSN numeric(25, 0),DifferentialBaseGUID uniqueidentifier,BackupTypeDescription nvarchar(60),BackupSetGUID uniqueidentifier,CompressedBackupSize bigint,Containment tinyint,KeyAlgorithm nvarchar(32),EncryptorThumbprint varbinary(20),EncryptorType nvarchar(32))
	INSERT INTO @BackupHistory EXEC ('RESTORE HEADERONLY FROM DISK = N''Z:\DB_Backup\AvantiAtlasSuite\AtlasIntegrationTests.bak''')
	--comment appropriately to get last backup or something else
	--declare @backupPositionToRestore int = 2
	set @backupPositionToRestore = (SELECT top(1)Position FROM @BackupHistory ORDER BY BackupStartDate DESC)
 
	print 'Restoring AtlasIntegrationTests'
	alter database AtlasIntegrationTests set single_user with rollback immediate;
	RESTORE DATABASE AtlasIntegrationTests FROM  DISK = N'Z:\DB_Backup\AvantiAtlasSuite\AtlasIntegrationTests.bak' WITH  FILE = @backupPositionToRestore,  NOUNLOAD,  STATS = 10
	alter database AtlasIntegrationTests set multi_user
 
	print 'Restoring AtlasIntegrationTestsNET'
	alter database AtlasIntegrationTestsNET set single_user with rollback immediate;
	RESTORE DATABASE AtlasIntegrationTestsNET FROM  DISK = N'Z:\DB_Backup\AvantiAtlasSuite\AtlasIntegrationTestsNET.bak' WITH  FILE = @backupPositionToRestore,  NOUNLOAD,  STATS = 10
	alter database AtlasIntegrationTestsNET set multi_user
 
	USE AtlasIntegrationTests
	print 'Finsished Restore'
end