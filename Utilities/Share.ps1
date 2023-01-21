# Look here for yours HKCU:Control Panel\Desktop\PerMonitorSettings\$MonitorID
$MonitorID = "SAM0F94H1AK500000_29_07E2_FB^58AA1CE198522B251C156F0A13BD8051"

function Restart-VideoDriver {
	param
	(
		[Parameter(Mandatory)][string]$GPUID
	)

	# Enables then disables GPU driver.
	Get-PnpDevice -FriendlyName $GPUID | Disable-PnpDevice -Confirm:$False
	Get-PnpDevice -FriendlyName $GPUID | Enable-PnpDevice -Confirm:$False
}

function Set-DPIScaling {
	param
	(
		[Parameter(Mandatory)][string]$MonitorID,
		[Parameter(Mandatory)][int]$ScalingLevel
	)

	Set-ItemProperty -Path "HKCU:Control Panel\Desktop\PerMonitorSettings\$MonitorID" -Name "DpiValue" -Value $ScalingLevel
}

function Get-VideoCard {
	$videoDevices = Get-PnpDevice -Class Display

	if ($videoDevices.Count -gt 1) {
		return $videoDevices[0].Name
	}
	else { 
		return $videoDevices.Name 
	}
}

function Main {
	$GPUID = Get-VideoCard
	Set-DPIScaling -MonitorID $MonitorID -ScalingLevel 0
	# Restart device driver.
	Restart-VideoDriver -GPUID $GPUID
}

Main