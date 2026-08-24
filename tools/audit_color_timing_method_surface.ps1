param(
    [string]$SourceProject = 'D:/unity/UnityProject/ColorTimeing/ColorTimeing',
    [string]$TargetProject = (Split-Path -Parent $PSScriptRoot),
    [string]$RoslynRoot = '',
    [string]$OutputPath = 'Documentation/Refactor/method-surface-audit.json'
)

$ErrorActionPreference = 'Stop'

function Resolve-RoslynRoot {
    param([string]$RequestedRoot)

    if ($RequestedRoot) {
        return (Resolve-Path -LiteralPath $RequestedRoot).Path
    }

    $matchingEditor = Get-Process Unity -ErrorAction SilentlyContinue |
        Where-Object { $_.MainWindowTitle -like '*_ColorTiming*' -and $_.Path } |
        Select-Object -First 1
    if ($matchingEditor) {
        $candidate = Join-Path (Split-Path -Parent $matchingEditor.Path) 'Data/Tools/ScriptUpdater'
        if (Test-Path -LiteralPath (Join-Path $candidate 'Microsoft.CodeAnalysis.CSharp.dll')) {
            return $candidate
        }
    }

    $editorsRoot = 'C:/Program Files/Unity/Hub/Editor'
    $candidate = Get-ChildItem -LiteralPath $editorsRoot -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending |
        ForEach-Object { Join-Path $_.FullName 'Editor/Data/Tools/ScriptUpdater' } |
        Where-Object { Test-Path -LiteralPath (Join-Path $_ 'Microsoft.CodeAnalysis.CSharp.dll') } |
        Select-Object -First 1
    if (!$candidate) {
        throw 'Could not locate Unity Roslyn assemblies. Pass -RoslynRoot explicitly.'
    }
    return $candidate
}

function Get-MethodSignatures {
    param([string]$Path)

    $sourceText = [IO.File]::ReadAllText($Path)
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($sourceText)
    return @(
        $tree.GetRoot().DescendantNodes() |
            Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] } |
            ForEach-Object {
                $parameterTypes = @($_.ParameterList.Parameters | ForEach-Object { $_.Type.ToString() })
                '{0}({1})' -f $_.Identifier.Text, ($parameterTypes -join ',')
            }
    )
}

function Compare-StringSets {
    param([string[]]$Left, [string[]]$Right)

    return @($Left | Where-Object { $Right -notcontains $_ } | Sort-Object -Unique)
}

$removedScripts = @{
    'Anim/AnimStateMachine_DMD.cs' = 'unreferenced animation prototype'
    'Anim/PlayAnimation.cs' = 'unreferenced no-op prototype'
    'GameManager.cs' = 'replaced by persistent IColorTimingSettings/GF.Setting'
    'PlayerInput.cs' = 'empty prototype; replaced by IGameInput'
    'Skill/Skill_Jiandao.cs' = 'empty prototype; formal attack path retained elsewhere'
    'Weapon_Hero.cs' = 'empty prototype'
    'ZZZZZZZZZZ.cs' = 'unreferenced Spine bone debug probe'
}

$allowedMissing = @{
    'Boss1_Controller.cs' = @('CreateHP_ctp(ColorType,int)')
    'Boss1SoundManager.cs' = @('Start()', 'Update()')
    'Boss2_Controller.cs' = @('CreateHP_ctp(ColorType,int)')
    'Boss2SoundManager.cs' = @('Start()', 'Update()')
    'HeroAnimStae.cs' = @('StartSupHit()')
    'HeroController.cs' = @('BulletTime()', 'GetIntType()')
    'HeroSoundManager.cs' = @('Start()')
    'Pickup_Weapon.cs' = @('Start()', 'OnTriggerStay2D(Collider2D)', 'TrayPickUP()')
    'WeaponControSystem.cs' = @(
        'Start()', 'Update()', 'OnBossDamage()', 'CheckWeapon()', 'CheckWeaponTip()',
        'CreateWeapon(ColorType,WeaponType)', 'GetRandomPos()',
        'CreateWeapon_dis(Vector3,ColorType,WeaponType)'
    )
    'WeaponControSystem_2.cs' = @(
        'Start()', 'Update()', 'OnBossDamage()', 'CheckWeapon()', 'CheckWeaponTip()',
        'CreateWeapon(ColorType,WeaponType)', 'GetRandomPos()',
        'CreateWeapon_dis(Vector3,ColorType,WeaponType)'
    )
    'Skill/sk_bo2_luodian.cs' = @('End()')
    'Skill/Skill_Bo1_Atk5_b.cs' = @('Start()', 'Update()')
    'Skill/Skill_Bo2_atk2_s.cs' = @('Start()')
    'Skill/Skill_Bo2_Atk2.cs' = @('SetSkill_Atk2()')
    'Skill/Skill_Bo2w_Atk.cs' = @('GetW2()')
    'Skill/Skill_Zhadan.cs' = @('Start()')
    'UI/LoadScenes.cs' = @(
        'FixedUpdate()', 'LodOK(AsyncOperation)', 'Fead(bool)', 'LoadScenesSync(string)'
    )
    'UI/StartVido.cs' = @('Start()', 'Update()', 'Startred()')
    'UI/UI_Game.cs' = @('Awake()', 'StartFead(bool)', 'GOBoss2()', 'GoStart()')
    'UI/UI_SoundManager.cs' = @('Awake()', 'Start()')
    'UI/UI_WeaponTip.cs' = @('TimeOK()', 'WaitForTime(float)')
}

$resolvedRoslynRoot = Resolve-RoslynRoot $RoslynRoot
Add-Type -Path (Join-Path $resolvedRoslynRoot 'Microsoft.CodeAnalysis.dll') -ErrorAction SilentlyContinue
Add-Type -Path (Join-Path $resolvedRoslynRoot 'Microsoft.CodeAnalysis.CSharp.dll') -ErrorAction SilentlyContinue

$sourceRoot = Join-Path $SourceProject 'Assets/Game/Scripts'
$targetRoot = Join-Path $TargetProject 'Assets/Game/Scripts/ColorTiming/Legacy'
if (!(Test-Path -LiteralPath $sourceRoot)) {
    throw "Source script root does not exist: $sourceRoot"
}
if (!(Test-Path -LiteralPath $targetRoot)) {
    throw "Target script root does not exist: $targetRoot"
}

$records = [Collections.Generic.List[object]]::new()
foreach ($sourceFile in Get-ChildItem -LiteralPath $sourceRoot -Recurse -Filter '*.cs' | Sort-Object FullName) {
    $relative = $sourceFile.FullName.Substring($sourceRoot.Length + 1).Replace('\', '/')
    $sourceMethods = @(Get-MethodSignatures $sourceFile.FullName)

    if ($removedScripts.ContainsKey($relative)) {
        $records.Add([ordered]@{
            script = $relative
            disposition = 'removed'
            reason = $removedScripts[$relative]
            sourceMethodCount = $sourceMethods.Count
            targetMethodCount = 0
            missingSourceMethods = $sourceMethods
            allowedMissingMethods = $sourceMethods
            unexpectedMissingMethods = @()
            staleAllowedExceptions = @()
        })
        continue
    }

    $targetPath = Join-Path $targetRoot $relative
    if (!(Test-Path -LiteralPath $targetPath)) {
        $records.Add([ordered]@{
            script = $relative
            disposition = 'missing-target'
            reason = ''
            sourceMethodCount = $sourceMethods.Count
            targetMethodCount = 0
            missingSourceMethods = $sourceMethods
            allowedMissingMethods = @()
            unexpectedMissingMethods = $sourceMethods
            staleAllowedExceptions = @()
        })
        continue
    }

    $targetMethods = @(Get-MethodSignatures $targetPath)
    $missing = @(Compare-StringSets $sourceMethods $targetMethods)
    $allowed = if ($allowedMissing.ContainsKey($relative)) { @($allowedMissing[$relative]) } else { @() }
    $unexpected = @(Compare-StringSets $missing $allowed)
    $stale = @(Compare-StringSets $allowed $missing)
    $records.Add([ordered]@{
        script = $relative
        disposition = if ($missing.Count -gt 0) { 'refactored-method-surface' } else { 'source-surface-retained' }
        reason = ''
        sourceMethodCount = $sourceMethods.Count
        targetMethodCount = $targetMethods.Count
        missingSourceMethods = $missing
        allowedMissingMethods = $allowed
        unexpectedMissingMethods = $unexpected
        staleAllowedExceptions = $stale
    })
}

$missingTargets = @($records | Where-Object { $_.disposition -eq 'missing-target' })
$unexpectedRecords = @($records | Where-Object { $_.unexpectedMissingMethods.Count -gt 0 })
$staleRecords = @($records | Where-Object { $_.staleAllowedExceptions.Count -gt 0 })
$sourceCountMatches = $records.Count -eq 64
$removedCountMatches = @($records | Where-Object { $_.disposition -eq 'removed' }).Count -eq 7
$retainedCountMatches = @($records | Where-Object { $_.disposition -ne 'removed' }).Count -eq 57
$changedCountMatches = @($records | Where-Object { $_.disposition -eq 'refactored-method-surface' }).Count -eq 21
$passed = $sourceCountMatches -and $removedCountMatches -and $retainedCountMatches -and
    $changedCountMatches -and $missingTargets.Count -eq 0 -and
    $unexpectedRecords.Count -eq 0 -and $staleRecords.Count -eq 0

$report = [ordered]@{
    status = if ($passed) { 'PASS' } else { 'FAIL' }
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    sourceProject = (Resolve-Path -LiteralPath $SourceProject).Path
    targetProject = (Resolve-Path -LiteralPath $TargetProject).Path
    roslynRoot = $resolvedRoslynRoot
    sourceScriptCount = $records.Count
    removedScriptCount = @($records | Where-Object { $_.disposition -eq 'removed' }).Count
    retainedScriptCount = @($records | Where-Object { $_.disposition -ne 'removed' }).Count
    unchangedSurfaceScriptCount = @($records | Where-Object { $_.disposition -eq 'source-surface-retained' }).Count
    refactoredSurfaceScriptCount = @($records | Where-Object { $_.disposition -eq 'refactored-method-surface' }).Count
    missingTargetCount = $missingTargets.Count
    unexpectedMissingCount = @($unexpectedRecords | ForEach-Object { $_.unexpectedMissingMethods }).Count
    staleAllowedExceptionCount = @($staleRecords | ForEach-Object { $_.staleAllowedExceptions }).Count
    countChecks = [ordered]@{
        source64 = $sourceCountMatches
        removed7 = $removedCountMatches
        retained57 = $retainedCountMatches
        refactoredSurface21 = $changedCountMatches
    }
    failures = @(
        $records | Where-Object {
            $_.disposition -eq 'missing-target' -or
            $_.unexpectedMissingMethods.Count -gt 0 -or
            $_.staleAllowedExceptions.Count -gt 0
        }
    )
    records = $records
}

$resolvedOutput = if ([IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath
} else {
    Join-Path $TargetProject $OutputPath
}
$outputDirectory = Split-Path -Parent $resolvedOutput
if (!(Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}
$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $resolvedOutput -Encoding utf8
[pscustomobject]$report | Select-Object status, sourceScriptCount, removedScriptCount, retainedScriptCount,
    unchangedSurfaceScriptCount, refactoredSurfaceScriptCount, missingTargetCount,
    unexpectedMissingCount, staleAllowedExceptionCount | Format-List

if (!$passed) {
    exit 1
}
