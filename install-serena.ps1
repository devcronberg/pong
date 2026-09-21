#Requires -Version 5.1
<#
.SYNOPSIS
Installs the optional Serena MCP prerequisites for this workspace on Windows.
.DESCRIPTION
Uses the pinned launcher arguments in .vscode/mcp.json. Downloads uv when needed
and prepares Python and Serena in the user cache without starting an MCP server.
#>
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) {
    throw 'This setup script supports Windows only.'
}

$configPath = Join-Path $PSScriptRoot '.vscode/mcp.json'
$server = (Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json).servers.serena
if ($server.command -ne 'uvx' -or $server.type -ne 'stdio') {
    throw 'Expected a stdio Serena server launched with uvx in .vscode/mcp.json.'
}
$serverArguments = @($server.args | ForEach-Object { $_.Replace('${workspaceFolder}', $PSScriptRoot) })
if (-not ($serverArguments -match '^serena-agent==\d+\.\d+\.\d+$')) {
    throw 'The MCP configuration must pin a serena-agent version.'
}

$env:PATH = @(
    $env:PATH
    [Environment]::GetEnvironmentVariable('Path', 'User')
    [Environment]::GetEnvironmentVariable('Path', 'Machine')
) -join ';'
$uvx = Get-Command uvx -CommandType Application -ErrorAction SilentlyContinue
if (-not $uvx) {
    $installDirectory = Join-Path $HOME '.local\bin'
    $existingLauncher = Join-Path $installDirectory 'uvx.exe'
    if (-not (Test-Path -LiteralPath $existingLauncher)) {
        Write-Host 'Installing uv in the current user profile from https://astral.sh/uv/install.ps1 ...'
        $previousInstallDirectory = $env:UV_INSTALL_DIR
        $previousSecurityProtocol = [Net.ServicePointManager]::SecurityProtocol
        try {
            $env:UV_INSTALL_DIR = $installDirectory
            [Net.ServicePointManager]::SecurityProtocol = $previousSecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
            $installer = Invoke-RestMethod -Uri 'https://astral.sh/uv/install.ps1'
            & ([scriptblock]::Create($installer))
        } finally {
            $env:UV_INSTALL_DIR = $previousInstallDirectory
            [Net.ServicePointManager]::SecurityProtocol = $previousSecurityProtocol
        }
    }
    if (-not (Test-Path -LiteralPath $existingLauncher)) {
        throw 'uv installation failed: uvx.exe was not found.'
    }
    $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    if (($userPath -split ';') -notcontains $installDirectory) {
        [Environment]::SetEnvironmentVariable('Path', "$installDirectory;$userPath", 'User')
    }
    $env:PATH = "$installDirectory;$env:PATH"
    $uvx = Get-Command uvx -CommandType Application -ErrorAction Stop
}

Write-Host "Using $($uvx.Source)"
& $uvx.Source --version
if ($LASTEXITCODE -ne 0) {
    throw "uvx failed with exit code $LASTEXITCODE."
}

Write-Host 'Preparing the configured Python and Serena versions, then checking the MCP command ...'
& $uvx.Source @serverArguments --help
if ($LASTEXITCODE -ne 0) {
    throw "Serena setup failed with exit code $LASTEXITCODE. Check network access and the output above."
}

Write-Host 'Serena launcher is ready. No MCP connection or C# symbol lookup has been tested yet.'
Write-Host 'Close all VS Code windows and reopen the project so VS Code picks up PATH changes.'
Write-Host 'Then run MCP: List Servers, select serena, and start/approve the server.'