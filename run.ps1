# Starts a log-viewer window, then launches the game.
# The log window tails game.log in real-time using Get-Content -Wait.

$logFile = Join-Path $PSScriptRoot "game.log"
$ErrorActionPreference = 'Stop'

# Ensure the log file exists before the viewer tries to open it
if (-not (Test-Path $logFile)) { New-Item $logFile -ItemType File | Out-Null }

# Open a separate PowerShell window that follows the log file live
$escapedLogFile = $logFile.Replace("'", "''")
$cmd = "Write-Host 'PONG log viewer' -ForegroundColor Cyan; Get-Content -LiteralPath '$escapedLogFile' -Encoding UTF8 -Wait -Tail 20"
$encodedCommand = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($cmd))
Start-Process powershell -ArgumentList '-NoProfile', '-NoExit', '-EncodedCommand', $encodedCommand

$previousLogPath = $env:PONG_LOG_PATH
$env:PONG_LOG_PATH = $logFile
Push-Location $PSScriptRoot
try {
	dotnet run --project Pong.csproj
} finally {
	Pop-Location
	$env:PONG_LOG_PATH = $previousLogPath
}
