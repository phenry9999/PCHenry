if (get-PSDrive D -ErrorAction SilentlyContinue) {
	Invoke-Sqlcmd -ServerInstance localhost -InputFile "C:\dev\sql\BackupDbs.sql" -verbose -QueryTimeout 0
}
else {
	write-output "External harddrive not connected, please connect drive and ensure it's drive letter is mapped to D"
}