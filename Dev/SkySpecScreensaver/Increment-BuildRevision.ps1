$revisionPath = Join-Path $PSScriptRoot 'build-revision.txt'
$attemptsRemaining = 20

while ($attemptsRemaining -gt 0) {
    try {
        $stream = [System.IO.File]::Open(
            $revisionPath,
            [System.IO.FileMode]::OpenOrCreate,
            [System.IO.FileAccess]::ReadWrite,
            [System.IO.FileShare]::None)

        try {
            $reader = [System.IO.StreamReader]::new($stream, [System.Text.Encoding]::UTF8, $true, 1024, $true)
            $text = $reader.ReadToEnd().Trim()
            $reader.Dispose()

            $revision = 0
            if ($text -and -not [int]::TryParse($text, [ref]$revision)) {
                throw "Invalid build revision '$text' in $revisionPath."
            }

            $stream.Position = 0
            $stream.SetLength(0)
            $writer = [System.IO.StreamWriter]::new($stream, [System.Text.UTF8Encoding]::new($false), 1024, $true)
            $writer.WriteLine($revision + 1)
            $writer.Flush()
            $writer.Dispose()
            exit 0
        }
        finally {
            $stream.Dispose()
        }
    }
    catch [System.IO.IOException] {
        $attemptsRemaining--
        if ($attemptsRemaining -eq 0) {
            throw
        }

        Start-Sleep -Milliseconds 100
    }
}
