#requires -Version 5.1
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$testRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("pong claude test " + [guid]::NewGuid())
$null = New-Item -ItemType Directory -Path $testRoot

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

function Get-TreeHashes([string]$Path) {
    @(Get-ChildItem -LiteralPath $Path -File -Recurse -Force | Sort-Object FullName | ForEach-Object {
        $_.FullName.Substring($Path.Length) + ':' + (Get-FileHash -LiteralPath $_.FullName).Hash
    }) -join "`n"
}

try {
    Copy-Item -LiteralPath (Join-Path $repoRoot '.github') -Destination $testRoot -Recurse
    Copy-Item -LiteralPath (Join-Path $repoRoot 'generate-claude.ps1') -Destination $testRoot
    $generator = Join-Path $testRoot 'generate-claude.ps1'
    $sourceRoot = Join-Path $testRoot '.github'
    $claudeRoot = Join-Path $testRoot '.claude'
    $instructionsPath = Join-Path $testRoot 'CLAUDE.md'
    $originalHashes = Get-TreeHashes $sourceRoot

    Copy-Item -LiteralPath (Join-Path $sourceRoot 'copilot-instructions.md') -Destination $instructionsPath
    $existingHash = (Get-FileHash -LiteralPath $instructionsPath).Hash
    $refused = $false
    try { & $generator } catch { $refused = $_.Exception.Message -like 'Refusing to overwrite*' }
    Assert-True $refused 'Generator must refuse to overwrite an existing CLAUDE.md.'
    Assert-True ((Get-FileHash -LiteralPath $instructionsPath).Hash -eq $existingHash) 'Existing instructions changed.'
    Remove-Item -LiteralPath $instructionsPath

    Push-Location ([System.IO.Path]::GetTempPath())
    try { & $generator } finally { Pop-Location }
    $instructions = [System.IO.File]::ReadAllText((Join-Path $sourceRoot 'copilot-instructions.md'))
    Assert-True ([System.IO.File]::ReadAllText($instructionsPath).EndsWith($instructions)) 'Instructions must preserve the original content.'
    Assert-True ((Get-TreeHashes (Join-Path $sourceRoot 'skills')) -eq (Get-TreeHashes (Join-Path $claudeRoot 'skills'))) 'Skill copies differ from their originals.'
    $agent = [System.IO.File]::ReadAllText((Join-Path $claudeRoot 'agents/monogame-dev.md'))
    Assert-True ($agent -match '\A---\r?\nname: monogame-dev\r?\n') 'Claude agent name or frontmatter missing.'
    Assert-True ($agent -match '(?m)^tools: Skill, Read, Grep, Glob, Edit, Write, Bash, PowerShell, WebFetch, WebSearch\r?$') 'Claude tools were not translated correctly.'
    $sourceAgent = [System.IO.File]::ReadAllText((Join-Path $sourceRoot 'agents/monogame-dev.agent.md'))
    Assert-True ($agent.EndsWith(($sourceAgent -split '---\r?\n', 3)[2])) 'Agent body changed.'

    Copy-Item -LiteralPath (Join-Path $repoRoot '.gitignore') -Destination (Join-Path $claudeRoot 'local.txt')
    $before = Get-TreeHashes $claudeRoot
    $instructionsHash = (Get-FileHash -LiteralPath $instructionsPath).Hash
    & $generator
    Assert-True ((Get-TreeHashes $claudeRoot) -eq $before) 'Repeated generation changed output or an unrelated Claude file.'
    Assert-True ((Get-FileHash -LiteralPath $instructionsPath).Hash -eq $instructionsHash) 'Repeated generation changed instructions.'
    Assert-True ((Get-TreeHashes $sourceRoot) -eq $originalHashes) 'Generator modified the Copilot sources.'

    Remove-Item -LiteralPath (Join-Path $sourceRoot 'skills/add-screen') -Recurse
    Remove-Item -LiteralPath (Join-Path $sourceRoot 'agents/monogame-dev.agent.md')
    & $generator
    Assert-True (-not (Test-Path -LiteralPath (Join-Path $claudeRoot 'skills/add-screen'))) 'Obsolete skill remains.'
    Assert-True (-not (Test-Path -LiteralPath (Join-Path $claudeRoot 'agents/monogame-dev.md'))) 'Obsolete agent remains.'
    Assert-True (Test-Path -LiteralPath (Join-Path $claudeRoot 'local.txt')) 'Unrelated Claude file was deleted.'

    Copy-Item -LiteralPath (Join-Path $repoRoot '.github/copilot-instructions.md') -Destination (Join-Path $sourceRoot 'agents/invalid.agent.md')
    $before = Get-TreeHashes $claudeRoot
    $refused = $false
    try { & $generator } catch { $refused = $_.Exception.Message -like 'Unsupported agent frontmatter*' }
    Assert-True $refused 'Unsupported agent frontmatter must fail.'
    Assert-True ((Get-TreeHashes $claudeRoot) -eq $before) 'Invalid input modified existing output.'
    Write-Host 'All Claude generation checks passed.'
} finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}