# Release Package Creation Script for CANopenEditor
# Creates deployment-ready ZIP files for both .NET Framework 4.8.1 and .NET 8.0

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ".\releases"
)

Write-Host "CANopenEditor Release Packaging" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Get version from version.txt if available
$versionFile = ".\EDSEditorGUI\version.txt"
$version = "1.0.0"
if (Test-Path $versionFile) {
    $version = (Get-Content $versionFile -First 1).Trim()
    Write-Host "Version: $version" -ForegroundColor Green
}

# Create output directory for releases
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Write-Host "Created output directory: $OutputDir" -ForegroundColor Yellow
}

# Define build outputs
$builds = @(
    @{
        Name = "net481"
        SourcePath = "EDSEditorGUI\bin\$Configuration\net481"
        ZipName = "EDSEditor-v$version-net481.zip"
        Description = ".NET Framework 4.8.1"
    },
    @{
        Name = "net8.0-windows"
        SourcePath = "EDSEditorGUI\bin\$Configuration\net8.0-windows"
        ZipName = "EDSEditor-v$version-net8.0-windows.zip"
        Description = ".NET 8.0"
    }
)

# Process each build
$successCount = 0
foreach ($build in $builds) {
    Write-Host ""
    Write-Host "Processing $($build.Description)..." -ForegroundColor Yellow
    
    $sourcePath = $build.SourcePath
    $destZip = Join-Path $OutputDir $build.ZipName
    
    if (-not (Test-Path $sourcePath)) {
        Write-Host "  ✗ Source path not found: $sourcePath" -ForegroundColor Red
        Write-Host "    Run 'dotnet build -c $Configuration' first" -ForegroundColor Red
        continue
    }
    
    # Remove old zip if exists
    if (Test-Path $destZip) {
        Remove-Item $destZip -Force
        Write-Host "  • Removed existing package" -ForegroundColor Gray
    }
    
    try {
        # Create ZIP file
        Write-Host "  • Creating package..." -ForegroundColor Gray
        Compress-Archive -Path "$sourcePath\*" -DestinationPath $destZip -CompressionLevel Optimal
        
        $zipSize = (Get-Item $destZip).Length / 1MB
        Write-Host "  ✓ Package created: $($build.ZipName)" -ForegroundColor Green
        Write-Host "    Size: $([math]::Round($zipSize, 2)) MB" -ForegroundColor Gray
        
        $successCount++
    }
    catch {
        Write-Host "  ✗ Failed to create package: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
if ($successCount -eq $builds.Count) {
    Write-Host "All packages created successfully! ✓" -ForegroundColor Green
    Write-Host "Location: $OutputDir" -ForegroundColor Cyan
}
elseif ($successCount -gt 0) {
    Write-Host "Partial success: $successCount of $($builds.Count) packages created" -ForegroundColor Yellow
}
else {
    Write-Host "No packages created" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Deployment packages ready for distribution!" -ForegroundColor Green
