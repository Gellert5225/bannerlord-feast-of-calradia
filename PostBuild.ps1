[CmdletBinding()]
param (
    [Parameter(Mandatory)][string]$Name,
    [Parameter(Mandatory)][string]$Version,
    [Parameter(Mandatory)][string]$GameFolder,
    [string]$Config
)

# OutputPath in the .csproj already wrote the DLL into:
#   $GameFolder\Modules\$Name\bin\Win64_Shipping_Client\$Name.dll
# This script copies the rest of the module assets (SubModule.xml, ModuleData/)
# into that same module folder, doing macro substitution on SubModule.xml.

$ErrorActionPreference = "Stop"

$repoRoot   = $PSScriptRoot
$moduleRoot = Join-Path $GameFolder "Modules\$Name"

# Ensure the module folder exists (OutputPath creates it on build, but be defensive)
New-Item -ItemType Directory -Path $moduleRoot -Force | Out-Null

# Substitute $(Name) and $(Version) into SubModule.xml and write to the module folder.
$subModuleSrc  = Join-Path $repoRoot "SubModule.xml"
$subModuleDest = Join-Path $moduleRoot "SubModule.xml"
(Get-Content $subModuleSrc -Raw) `
    -replace '\$\(Name\)',    $Name `
    -replace '\$\(Version\)', $Version `
    | Set-Content -Path $subModuleDest -Encoding UTF8

# Mirror ModuleData/ if the source folder exists. Robocopy /MIR keeps the destination
# in sync (deletes files removed from source). Exit codes 0-7 are non-fatal for robocopy.
$moduleDataSrc  = Join-Path $repoRoot "ModuleData"
$moduleDataDest = Join-Path $moduleRoot "ModuleData"
if (Test-Path $moduleDataSrc) {
    robocopy $moduleDataSrc $moduleDataDest /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) {
        throw "robocopy failed with exit code $LASTEXITCODE"
    }
    $global:LASTEXITCODE = 0
}
