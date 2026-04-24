# Starts a log-viewer window, then launches the game.
# The log window tails game.log in real-time using Get-Content -Wait.

$logFile = Join-Path $PSScriptRoot "game.log"

# Ensure the log file exists before the viewer tries to open it
if (-not (Test-Path $logFile)) { New-Item $logFile -ItemType File | Out-Null }

# Open a separate PowerShell window that follows the log file live
$cmd = "Write-Host 'PONG log viewer' -ForegroundColor Cyan; Get-Content -Path `"$logFile`" -Wait -Tail 20"
Start-Process powershell -ArgumentList "-NoExit", "-Command", $cmd

# Give the log window a moment to open, then start the game
Start-Sleep -Milliseconds 500
Set-Location $PSScriptRoot
dotnet run --project Game1.csproj
