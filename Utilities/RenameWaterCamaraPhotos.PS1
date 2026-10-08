param(
    [Parameter(Mandatory = $true)]
    [string]$Folder
)

Add-Type -AssemblyName System.Drawing

function Get-PhotoDateTaken($FilePath) {
    try {
        $image = [System.Drawing.Image]::FromFile($FilePath)

        # EXIF tag 36867 = DateTimeOriginal
        $property = $image.GetPropertyItem(36867)

        $dateString = [System.Text.Encoding]::ASCII.GetString($property.Value).Trim([char]0)

        $image.Dispose()

        # EXIF format: yyyy:MM:dd HH:mm:ss
        return [datetime]::ParseExact(
            $dateString,
            "yyyy:MM:dd HH:mm:ss",
            $null
        )
    }
    catch {
        return $null
    }
}

$files = Get-ChildItem -Path $Folder -File |
Where-Object { $_.Extension -match "^\.(jpg|jpeg)$" -and $_.Name -like "DSCF*" }

foreach ($file in $files) {

    $dateTaken = Get-PhotoDateTaken $file.FullName

    if ($null -eq $dateTaken) {
        Write-Warning "No EXIF date found: $($file.Name)"
        continue
    }

    # Pixel filename format
    $newName = "PXL_{0}_{1}.jpg" -f `
        $dateTaken.ToString("yyyyMMdd"),
    $dateTaken.ToString("HHmmssfff")

    $newPath = Join-Path $Folder $newName

    # Avoid collisions
    if (Test-Path $newPath) {
        Write-Warning "Already exists: $newName"
        continue
    }

    Rename-Item -Path $file.FullName -NewName $newName

    Write-Host "$($file.Name) -> $newName"
}