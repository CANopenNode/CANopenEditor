param(
    [Parameter(Mandatory = $true)]
    [string[]]$FixtureDirectory,
    [Parameter(Mandatory = $true)]
    [string]$LegacyStackDirectory,
    [Parameter(Mandatory = $true)]
    [string]$V4StackDirectory,
    [ValidateSet('x64', 'Win32')]
    [string]$Platform = 'x64',
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath "$LegacyStackDirectory/stack/CO_SDO.h") -or
    -not (Test-Path -LiteralPath "$V4StackDirectory/301/CO_ODinterface.h"))
{
    throw 'Supply matching CANopenNode legacy (v1.3) and v4 (v4.1) source checkouts.'
}
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$installation = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath | Select-Object -First 1
$cmake = Join-Path $installation 'Common7/IDE/CommonExtensions/Microsoft/CMake/CMake/bin/cmake.exe'
if (-not (Test-Path -LiteralPath $cmake)) { throw 'Install Visual Studio v145 and CMake tools.' }
$workspace = Join-Path $PSScriptRoot "generated-code-build/$Platform"
New-Item -ItemType Directory -Path $workspace -Force | Out-Null
$lines = [Collections.Generic.List[string]]::new()
$lines.Add('cmake_minimum_required(VERSION 3.20)')
$lines.Add('project(GeneratedOdRegressions LANGUAGES C)')
$lines.Add('set(CMAKE_C_STANDARD 11)')
$lines.Add('set(CMAKE_C_STANDARD_REQUIRED ON)')
function CMakePath([string]$Path)
{
    return '"' + [IO.Path]::GetFullPath($Path).Replace('\', '/').Replace('"', '\"') + '"'
}
$count = 0
foreach ($directory in $FixtureDirectory)
{
    foreach ($source in Get-ChildItem -LiteralPath $directory -Filter OD.c -Recurse -File)
    {
        $v4 = Select-String -LiteralPath $source.FullName -SimpleMatch '301/CO_ODinterface.h' -Quiet
        $includes = if ($v4) { @($V4StackDirectory, "$V4StackDirectory/example") }
        else { @($LegacyStackDirectory, "$LegacyStackDirectory/stack", "$LegacyStackDirectory/stack/drvTemplate") }
        $target = "od$count"
        $lines.Add("add_library($target OBJECT $(CMakePath $source.FullName))")
        $includePaths = ($includes | ForEach-Object { CMakePath $_ }) -join ' '
        $lines.Add("target_include_directories($target PRIVATE $includePaths)")
        $count++
    }
}
if ($count -eq 0) { throw 'No generated OD.c fixtures found. Run issue tests with EDS_REGRESSION_OUTPUT_DIRECTORY set.' }
[IO.File]::WriteAllLines((Join-Path $workspace 'CMakeLists.txt'), $lines)
& $cmake -S $workspace -B "$workspace/build" -G 'Visual Studio 18 2026' -A $Platform -T v145
if ($LASTEXITCODE -ne 0) { throw 'Generated-C configure failed.' }
& $cmake --build "$workspace/build" --config $Configuration --parallel 4
if ($LASTEXITCODE -ne 0) { throw 'Generated OD source failed to compile against the pinned CANopenNode headers.' }
Write-Output "PASS: $count generated C translation units, $Platform $Configuration"
