use master

declare @backupComment nvarchar(255) = 'Backup at ' + convert(varchar, getdate(), 120)

print 'Backing up FusebillMasterLocal' 
BACKUP DATABASE [FusebillMasterLocal] TO DISK = N'd:\FusebillDB\FusebillMasterLocal.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'FusebillMasterLocal-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up FusebillMasterLocal' 

print 'Backing up ElmahDBLocal' 
BACKUP DATABASE [ElmahDBLocal] TO DISK = N'd:\FusebillDB\ElmahDBLocal.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'ElmahDBLocal-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up ElmahDBLocal' 

print 'Backing up fb_CommPlatform_trinity' 
BACKUP DATABASE [fb_CommPlatform_trinity] TO DISK = N'd:\FusebillDB\fb_CommPlatform_trinity.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'fb_CommPlatform_trinity-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up fb_CommPlatform_trinity' 

print 'Backing up FBReportingLocal' 
BACKUP DATABASE [FBReportingLocal] TO DISK = N'd:\FusebillDB\FBReportingLocal.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'FBReportingLocal-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up FBReportingLocal' 

print 'Backing up fusebillPaymentRouter' 
BACKUP DATABASE [fusebillPaymentRouter] TO DISK = N'd:\FusebillDB\fusebillPaymentRouter.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'fusebillPaymentRouter-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up fusebillPaymentRouter' 

print 'Backing up FusebillSession' 
BACKUP DATABASE [FusebillSession] TO DISK = N'd:\FusebillDB\FusebillSession.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'FusebillSession-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up FusebillSession' 

print 'Backing up SSISDB' 
BACKUP DATABASE [SSISDB] TO DISK = N'd:\FusebillDB\SSISDB.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'SSISDB-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up SSISDB' 

print 'Backing up Vault' 
BACKUP DATABASE [Vault] TO DISK = N'd:\FusebillDB\Vault.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'Vault-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up Vault' 

print 'Backing up WebLogsLocal' 
BACKUP DATABASE [WebLogsLocal] TO DISK = N'd:\FusebillDB\WebLogsLocal.bak' WITH DESCRIPTION = @backupComment, NOINIT,  NAME = N'WebLogsLocal-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 
print 'Backed up WebLogsLocal' 
