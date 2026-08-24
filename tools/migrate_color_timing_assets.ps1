param(
    [Parameter(Mandatory = $false)]
    [string]$SourceProject = 'D:\unity\UnityProject\ColorTimeing\ColorTimeing',

    [Parameter(Mandatory = $false)]
    [string]$TargetProject = 'D:\unity\UnityProject\ColorTimeing\New\_ColorTiming',

    [switch]$DryRun,

    [switch]$PreserveModifiedTargets
)

$ErrorActionPreference = 'Stop'
$sourceAssets = Join-Path $SourceProject 'Assets'
$targetAssets = Join-Path $TargetProject 'Assets'
$records = [System.Collections.Generic.List[object]]::new()

if (-not (Test-Path -LiteralPath $sourceAssets -PathType Container)) {
    throw "Source Assets directory not found: $sourceAssets"
}
if (-not (Test-Path -LiteralPath $targetAssets -PathType Container)) {
    throw "Target Assets directory not found: $targetAssets"
}

function Get-RelativePath([string]$BasePath, [string]$FullPath) {
    $normalizedBase = [IO.Path]::GetFullPath($BasePath).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    $normalizedFull = [IO.Path]::GetFullPath($FullPath)
    if (-not $normalizedFull.StartsWith($normalizedBase, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the expected base: $normalizedFull"
    }
    return $normalizedFull.Substring($normalizedBase.Length)
}

function Add-FileMapping([string]$SourceFile, [string]$TargetFile) {
    if (-not (Test-Path -LiteralPath $SourceFile -PathType Leaf)) {
        throw "Mapped source file not found: $SourceFile"
    }

    $sourceHash = (Get-FileHash -LiteralPath $SourceFile -Algorithm SHA256).Hash
    $targetHash = $null
    $status = if ($DryRun) { 'DryRun' } else { 'Copied' }
    $shouldCopy = -not $DryRun
    if (Test-Path -LiteralPath $TargetFile -PathType Leaf) {
        $targetHash = (Get-FileHash -LiteralPath $TargetFile -Algorithm SHA256).Hash
        if ($sourceHash -ne $targetHash) {
            if (-not $PreserveModifiedTargets) {
                throw "Refusing to overwrite a different target file: $TargetFile"
            }
            $status = 'ModifiedTargetPreserved'
            $shouldCopy = $false
        }
        else {
            $status = 'Same'
            $shouldCopy = $false
        }
    }

    $records.Add([pscustomobject]@{
        Source = Get-RelativePath $SourceProject $SourceFile
        Target = Get-RelativePath $TargetProject $TargetFile
        Bytes = (Get-Item -LiteralPath $SourceFile).Length
        SourceSHA256 = $sourceHash
        TargetSHA256 = if ($targetHash) { $targetHash } else { $sourceHash }
        Status = $status
    })

    if ($shouldCopy) {
        $targetDirectory = Split-Path -Parent $TargetFile
        if (-not (Test-Path -LiteralPath $targetDirectory)) {
            New-Item -ItemType Directory -Path $targetDirectory -Force | Out-Null
        }
        Copy-Item -LiteralPath $SourceFile -Destination $TargetFile -Force
    }
}

function Add-TreeMapping([string]$SourceDirectory, [string]$TargetDirectory) {
    Get-ChildItem -LiteralPath $SourceDirectory -Recurse -File | ForEach-Object {
        $relative = Get-RelativePath $SourceDirectory $_.FullName
        Add-FileMapping $_.FullName (Join-Path $TargetDirectory $relative)
    }
}

function Add-FolderMetaMapping([string]$SourceFolder, [string]$TargetFolder) {
    Add-FileMapping "$SourceFolder.meta" "$TargetFolder.meta"
}

# Product content and its source folder GUID.
$sourceArt = Join-Path $sourceAssets 'Art'
$targetArt = Join-Path $targetAssets 'Game\ColorTiming\Art'
Get-ChildItem -LiteralPath $sourceArt -Recurse -File | ForEach-Object {
    $relative = Get-RelativePath $sourceArt $_.FullName
    if ($relative -ne 'Sound.meta' -and -not $relative.StartsWith('Sound\', [StringComparison]::OrdinalIgnoreCase)) {
        Add-FileMapping $_.FullName (Join-Path $targetArt $relative)
    }
}
Add-FolderMetaMapping $sourceArt $targetArt

$sourceSound = Join-Path $sourceArt 'Sound'
$targetSound = Join-Path $targetAssets 'Game\Audio\ColorTiming'
Add-TreeMapping $sourceSound $targetSound
Add-FolderMetaMapping $sourceSound $targetSound

# Scenes use the framework's canonical scene path; the existing Scene folder meta is retained.
$sourceScenes = Join-Path $sourceAssets 'Scenes'
$targetScenes = Join-Path $targetAssets 'Game\Scene'
Get-ChildItem -LiteralPath $sourceScenes -File | ForEach-Object {
    Add-FileMapping $_.FullName (Join-Path $targetScenes $_.Name)
}

# Prefabs are classified without changing individual prefab GUIDs.
$sourcePrefabs = Join-Path $sourceAssets 'Game\Prefba'
$targetEntityPrefabs = Join-Path $targetAssets 'Game\Prefabs\Entity\ColorTiming'
$targetUiPrefabs = Join-Path $targetAssets 'Game\Prefabs\UI\ColorTiming'
$targetWorldPrefabs = Join-Path $targetAssets 'Game\Prefabs\World\ColorTiming'

Get-ChildItem -LiteralPath $sourcePrefabs -File | Where-Object {
    $_.Extension -in '.prefab', '.meta' -and ($_.BaseName -notin @('Game', 'Scene', 'UI'))
} | ForEach-Object {
    Add-FileMapping $_.FullName (Join-Path $targetEntityPrefabs $_.Name)
}
Get-ChildItem -LiteralPath $sourcePrefabs -Directory | Where-Object {
    $_.Name -notin @('Game', 'Scene', 'UI')
} | ForEach-Object {
    Add-TreeMapping $_.FullName (Join-Path $targetEntityPrefabs $_.Name)
}
Add-FolderMetaMapping $sourcePrefabs $targetEntityPrefabs

$sourceUi = Join-Path $sourcePrefabs 'UI'
Add-TreeMapping $sourceUi $targetUiPrefabs
Add-FolderMetaMapping $sourceUi $targetUiPrefabs

$sourceGameUi = Join-Path $sourcePrefabs 'Game'
$targetGameUi = Join-Path $targetUiPrefabs 'Game'
Add-TreeMapping $sourceGameUi $targetGameUi
Add-FolderMetaMapping $sourceGameUi $targetGameUi

$sourceWorld = Join-Path $sourcePrefabs 'Scene'
Add-TreeMapping $sourceWorld $targetWorldPrefabs
Add-FolderMetaMapping $sourceWorld $targetWorldPrefabs

# Source-compatible Spine runtime and settings.
$sourceSpine = Join-Path $sourceAssets 'Plugins\Spine'
$targetSpine = Join-Path $targetAssets 'Plugins\Spine'
Add-TreeMapping $sourceSpine $targetSpine
Add-FolderMetaMapping $sourceSpine $targetSpine
foreach ($name in @('SpineSettings.asset', 'SpineSettings.asset.meta')) {
    Add-FileMapping (Join-Path $sourceAssets "Editor\$name") (Join-Path $targetAssets "Editor\$name")
}

# Preserve MonoScript GUIDs while placing legacy behavior behind a product-owned boundary.
$excludedScripts = @(
    'PlayerInput.cs',
    'Weapon_Hero.cs',
    'Skill\Skill_Jiandao.cs',
    'Anim\PlayAnimation.cs',
    'Anim\AnimStateMachine_DMD.cs',
    'ZZZZZZZZZZ.cs'
)
$sourceScripts = Join-Path $sourceAssets 'Game\Scripts'
$targetLegacyScripts = Join-Path $targetAssets 'Game\Scripts\ColorTiming\Legacy'
Get-ChildItem -LiteralPath $sourceScripts -Recurse -File | ForEach-Object {
    $relative = Get-RelativePath $sourceScripts $_.FullName
    $scriptRelative = if ($relative.EndsWith('.meta')) { $relative.Substring(0, $relative.Length - 5) } else { $relative }
    if ($excludedScripts -notcontains $scriptRelative) {
        Add-FileMapping $_.FullName (Join-Path $targetLegacyScripts $relative)
    }
}
Add-FolderMetaMapping $sourceScripts $targetLegacyScripts

$duplicates = $records | Group-Object Target | Where-Object Count -gt 1
if ($duplicates) {
    throw "Duplicate target mappings: $($duplicates.Name -join ', ')"
}

$mappedPrefabs = ($records | Where-Object { $_.Source -like '*.prefab' }).Count
if ($mappedPrefabs -ne 48) {
    throw "Expected 48 mapped prefabs, got $mappedPrefabs"
}

if (-not $DryRun) {
    $manifest = Join-Path $TargetProject 'Documentation\Refactor\Baseline\migrated-assets.csv'
    $records | Sort-Object Target | Export-Csv -LiteralPath $manifest -NoTypeInformation -Encoding utf8
}

[pscustomobject]@{
    DryRun = [bool]$DryRun
    Files = $records.Count
    Bytes = ($records | Measure-Object -Property Bytes -Sum).Sum
    Prefabs = $mappedPrefabs
    ModifiedTargetsPreserved = ($records | Where-Object Status -eq 'ModifiedTargetPreserved').Count
}
