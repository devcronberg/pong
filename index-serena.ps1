#Requires -Version 5.1
<#
.SYNOPSIS
Builds or refreshes the local Serena symbol index for this repository.
.DESCRIPTION
Uses the Python and Serena versions from .vscode/mcp.json and the exclusions in
.serena/project.yml. Run install-serena.ps1 first. No MCP server is started.
#>
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$configPath = Join-Path $PSScriptRoot '.vscode/mcp.json'
$server = (Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json).servers.serena
$commandIndex = [Array]::IndexOf($server.args, 'start-mcp-server')
if ($server.command -ne 'uvx' -or $commandIndex -lt 1 -or
    $server.args[$commandIndex - 1] -ne 'serena' -or
    -not ($server.args -match '^serena-agent==\d+\.\d+\.\d+$')) {
    throw 'Expected a pinned Serena uvx command in .vscode/mcp.json.'
}
$launcherArguments = @($server.args[0..($commandIndex - 1)])
if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot '.serena/project.yml'))) {
    throw 'Missing .serena/project.yml. Restore the shared configuration before indexing.'
}
if (-not (Get-Command dotnet -CommandType Application -ErrorAction SilentlyContinue)) {
    throw 'Install the .NET SDK required by PONG before indexing C# code.'
}

$uvx = Get-Command uvx -CommandType Application -ErrorAction SilentlyContinue
if (-not $uvx) {
    $installedLauncher = Join-Path $HOME '.local/bin/uvx.exe'
    if (-not (Test-Path -LiteralPath $installedLauncher)) {
        throw 'uvx was not found. Run install-serena.ps1 first and reopen your terminal.'
    }
    $uvx = Get-Command $installedLauncher -CommandType Application -ErrorAction Stop
}

Push-Location $PSScriptRoot
try {
    Write-Host "Indexing $PSScriptRoot using the shared Serena exclusions ..."
    $indexingOutput = @()
    & $uvx.Source @launcherArguments project index $PSScriptRoot |
        Tee-Object -Variable indexingOutput
    if ($LASTEXITCODE -ne 0) {
        throw "Serena indexing failed with exit code $LASTEXITCODE."
    }
    if ($indexingOutput -match 'Failed to index \d+ files') {
        throw 'Serena could not index all files. See .serena/logs/indexing.txt for details.'
    }
    if (-not ($indexingOutput -match '^Indexed files per language:\s*\S')) {
        throw 'Serena did not report any indexed source files. Check the project configuration and output above.'
    }
} finally {
    Pop-Location
}

Write-Host 'Local Serena symbol index is ready. Cache and logs are ignored by Git.'
Write-Host 'This does not verify the VS Code MCP connection.'