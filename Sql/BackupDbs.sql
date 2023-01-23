use master

declare @backupComment nvarchar(255) = 'Backup at ' + convert(varchar, getdate(), 120)

print 'Backing up MasterLocal' 
BACKUP DATABASE [MasterLocal] TO DISK = N'd:\DB\MasterLocal.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'MasterLocal-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up MasterLocal'
