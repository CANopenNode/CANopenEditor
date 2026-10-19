param(
    [ValidateSet('x64', 'x86', 'All')]
    [string]$Platform = 'All',
    [ValidateSet('Debug', 'Release', 'All')]
    [string]$Configuration = 'All',
    [string]$EditorDirectory
)
$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'Install MSBuild and the .NET Framework 4.8 targeting pack.' }
if ($EditorDirectory -and ($Platform -eq 'All' -or $Configuration -eq 'All'))
{
    throw 'An explicit editor deployment requires one matching platform and configuration.'
}
$architectures = if ($Platform -eq 'All') { @('x64', 'x86') } else { @($Platform) }
$variants = if ($Configuration -eq 'All') { @('Debug', 'Release') } else { @($Configuration) }
foreach ($architecture in $architectures)
{
    foreach ($variant in $variants)
    {
        & $msbuild "$PSScriptRoot/EditorRegressionTests.csproj" /t:Build "/p:Platform=$architecture" "/p:Configuration=$variant" /nologo /v:minimal
        if ($LASTEXITCODE -ne 0) { throw 'Editor regression harness build failed.' }
        $deployment = if ($EditorDirectory) { $EditorDirectory } elseif ($architecture -eq 'x64')
        {
            "$PSScriptRoot/../EDSEditorGUI/bin/x64/$variant"
        }
        else { "$PSScriptRoot/../EDSEditorGUI/bin/$variant" }
        & "$PSScriptRoot/build/$architecture/$variant/EditorRegressionTests.exe" $deployment
        if ($LASTEXITCODE -ne 0) { throw "Editor regressions failed: $architecture $variant" }
    }
}
