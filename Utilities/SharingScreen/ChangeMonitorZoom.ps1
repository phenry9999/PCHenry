# useful info here: https://www.reddit.com/r/Batch/comments/4665jq/how_to_change_windows_10_display_scaling_via/d03gt72/
# "0" state represents the Recommended option. Since it's not possible to use negatives as registry values, OS uses complementary numbers instead. So -1 becomes 4294967295, -2 becomes 4294967294, and so on.

# Look here for yours HKCU:Control Panel\Desktop\PerMonitorSettings\$MonitorID
#Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers\ScaleFactors\
# in RegEdit, copy paste this in the top search Computer\HKEY_CURRENT_USER\Control Panel\Desktop\PerMonitorSettings
$MonitorID = "DELD10795RT6N3_1D_07E6_36^A06BE3E8D67DDA4539CC2D14689F6FD5"

#Run this command in powershell with admin privs and copy paste the Instance Id value
#pnputil /enum-devices /class Display
$DisplayId = "PCI\VEN_1002&DEV_743F&SUBSYS_50801462&REV_C1\6&311d164&0&00000008"

# To check these values, open up RegEd, goto path above, change zoom setting, open settings and check again
# 0 is likely the Recommended value, and they go up or down (hex) from there
$dpiValueDailyDev = "-2"
$dpiValueZoomed = "2"

function Mainline {
	param ([string]$mode)

	if ($mode -eq "Share") {
		$dpiValue = $dpiValueZoomed
	}
	else {
		$dpiValue = $dpiValueDailyDev
	}

	write-output "Display Mode is $mode with DPI $dpiValue"
	write-output "Changing video driver zoom level"
	Set-DPIScaling -MonitorID $MonitorID -ScalingLevel $dpiValue
	Restart-VideoDriver
	write-output "Changed video driver zoom level"
}

#to manually reset vid card, use Win + Ctrl + Shift + B
function Restart-VideoDriver {
	write-output("Restarting monitor")
	pnputil.exe /disable-device $DisplayId
	pnputil.exe /enable-device $DisplayId
	write-output("Restarted monitor")
}

function Set-DPIScaling {
	param([Parameter(Mandatory)][string]$MonitorID,	[Parameter(Mandatory)][int]$ScalingLevel)
	write-output("Updating HKCU:Control Panel\Desktop\PerMonitorSettings\$MonitorID to $ScalingLevel")
	Set-ItemProperty -Path "HKCU:Control Panel\Desktop\PerMonitorSettings\$MonitorID" -Name "DpiValue" -Value $ScalingLevel
	write-output("Updated HKCU:Control Panel\Desktop\PerMonitorSettings\$MonitorID to $ScalingLevel")
}

$mode = $args[0]
Mainline $mode