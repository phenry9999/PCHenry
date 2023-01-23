use master

declare @restoreSql varchar(2000)
Create Table #BackupDBData (BackupName nvarchar(128), BackupDescription nvarchar(255), BackupType smallint, ExpirationDate datetime, Compressed bit, Position smallint, DeviceType tinyint, UserName nvarchar(128), ServerName nvarchar(128), DatabaseName nvarchar(128), DatabaseVersion int, DatabaseCreationDate datetime, BackupSize numeric(20,0), FirstLSN numeric(25,0), LastLSN numeric(25,0), CheckpointLSN numeric(25,0), DatabaseBackupLSN numeric(25,0), BackupStartDate datetime, BackupFinishDate datetime, SortOrder smallint, CodePage smallint, UnicodeLocaleId int, UnicodeComparisonStyle int, CompatibilityLevel tinyint, SoftwareVendorId int, SoftwareVersionMajor int, SoftwareVersionMinor int, SoftwareVersionBuild int, MachineName nvarchar(128), Flags int, BindingID uniqueidentifier, RecoveryForkID uniqueidentifier, Collation nvarchar(128), FamilyGUID uniqueidentifier, HasBulkLoggedData bit, IsSnapshot bit, IsReadOnly bit, IsSingleUser bit, HasBackupChecksums bit, IsDamaged bit, BeginsLogChain bit, HasIncompleteMetaData bit, IsForceOffline bit, IsCopyOnly bit, FirstRecoveryForkID uniqueidentifier, ForkPointLSN numeric(25,0) NULL, RecoveryModel nvarchar(60), DifferentialBaseLSN numeric(25,0) NULL, DifferentialBaseGUID uniqueidentifier, BackupTypeDescription nvarchar(60), BackupSetGUID uniqueidentifier NULL, CompressedBackupSize bigint null, Containment tinyint, KeyAlgorithm nvarchar(32), EncryptorThumbprint varbinary(20), EncryptorType nvarchar(32)) 
declare @lastBackupFilePosition int
declare @dbBackupDate varchar(255) 
declare @dbName varchar(100)

set @dbName = 'MasterLocal'
print 'Restoring to ' + @dbName
insert #BackupDBData Exec ('RESTORE HEADERONLY FROM  DISK = N''d:\DB\' + @dbName + '.bak''') 
set @lastBackupFilePosition = (select max(Position) from #BackupDBData)
set @dbBackupDate = (select BackupFinishDate from #BackupDBData where Position = @lastBackupFilePosition)
print 'restoring from file position = ' + cast(@lastBackupFilePosition as varchar) + ' created on ' + cast(@dbBackupDate as varchar(255))
truncate table #BackupDBData
set @restoreSql = 'ALTER DATABASE [' + @dbName + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE '
--exec sp_executesql @restoreSql
set @restoreSql = 'RESTORE DATABASE ' + @dbName + ' FROM DISK = N''d:\DB\' + @dbName + '.bak'' WITH FILE = ' + convert(varchar, @lastBackupFilePosition) + ', MOVE ''' + @dbName +''' TO ''c:\DB\' + @dbName + '.mdf'', MOVE ''' + @dbName + ''' TO ''c:\DB\' + @dbName + '.ldf'', NOUNLOAD, REPLACE, STATS = 10 '
--exec sp_executesql @restoreSql
set @restoreSql = 'ALTER DATABASE [' + @dbName + '] SET MULTI_USER '
--exec sp_executesql @restoreSql

--back to making db usable\queryable
drop table #BackupDBData 
USE MasterLocal
