#start\restart the LOCAL websites

Import-Module WebAdministration

Stop-WebSite "local.APPNAME.com"
Start-WebSite "local.APPNAME.com"

#get process IDs, shows if they are indeed running afterwards
c:\windows\system32\inetsrv\appcmd.exe list wp