# VARIABLES

# You can find this in regedit under HKCU\Control Panel\Desktop\PerMonitorSettings
# NB: 
# - This isn't in the registry until you have changed it at least once already.
# - Which monitor are you targeting if there is more than one
# - Also pre-win10 it appears that the dpi value is stored in HKCU:\Control Panel\Desktop with the value LogPixels
# - Pre-win10 also requires a restart
# - useful info here: https://www.reddit.com/r/Batch/comments/4665jq/how_to_change_windows_10_display_scaling_via/d03gt72/
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

		# From my testing additional GPU's show up first in the object
		return $videoDevices[0].Name

	}
 else { return $videoDevices.Name }

}

function Main {
	$GPUID = Get-VideoCard

	# Set scaling to 150%.
	Set-DPIScaling -MonitorID $MonitorID -ScalingLevel -2

	# Restart device driver.
	Restart-VideoDriver -GPUID $GPUID
}

Main