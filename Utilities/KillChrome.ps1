$chrome = Get-Process chromedriver -ErrorAction SilentlyContinue

if ($chrome) {
	Write-Output "Chrome running, pulling rug out from underneath"
	#taskkill /im chromedriver.exe /f
	$chrome | Stop-Process -Force
}
else {
	Write-Output "Chrome not running"
}

Remove-Variable chrome