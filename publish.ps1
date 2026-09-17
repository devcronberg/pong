# Publishes the game as a single self-contained executable.
# Builds for both Windows x64 and Linux x64.
# Output is placed in: publish\win-x64\ and publish\linux-x64\

$projectFile = "Pong.csproj"
$runtimes    = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")
$failed      = @()

foreach ($runtime in $runtimes) {
    $outputDir = "publish\$runtime"
    Write-Host "Building for $runtime..." -ForegroundColor Cyan

    dotnet publish $projectFile `
        --configuration Release `
        --runtime $runtime `
        --self-contained true `
        /p:PublishSingleFile=true `
        --output $outputDir

    if ($LASTEXITCODE -ne 0) {
        $failed += $runtime
    }
}

Write-Host ""
if ($failed.Count -eq 0) {
    Write-Host "All builds succeeded." -ForegroundColor Green
    Write-Host "  publish\win-x64\Pong.exe   (Windows)"
    Write-Host "  publish\linux-x64\Pong     (Linux)"
    Write-Host "  publish\osx-x64\Pong       (macOS Intel)"
    Write-Host "  publish\osx-arm64\Pong     (macOS Apple Silicon)"
} else {
    Write-Host "Failed runtimes: $($failed -join ', ')" -ForegroundColor Red
    exit 1
}
