param(
    [string]$SourceProject = 'D:/unity/UnityProject/ColorTimeing/ColorTimeing',
    [string]$TargetProject = (Split-Path -Parent $PSScriptRoot),
    [string]$RoslynRoot = '',
    [string]$OutputPath = 'openspec/changes/migrate-color-timing-to-ai-friendly-framework/evidence/serialized-field-surface-audit.json'
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

    $candidate = Get-ChildItem -LiteralPath 'C:/Program Files/Unity/Hub/Editor' -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending |
        ForEach-Object { Join-Path $_.FullName 'Editor/Data/Tools/ScriptUpdater' } |
        Where-Object { Test-Path -LiteralPath (Join-Path $_ 'Microsoft.CodeAnalysis.CSharp.dll') } |
        Select-Object -First 1
    if (!$candidate) {
        throw 'Could not locate Unity Roslyn assemblies. Pass -RoslynRoot explicitly.'
    }
    return $candidate
}

function Get-SimpleTypeName {
    param([string]$TypeName)

    return $TypeName.Split('.')[-1]
}

function Build-ClassMap {
    param([string]$Root)

    $map = @{}
    foreach ($file in Get-ChildItem -LiteralPath $Root -Recurse -Filter '*.cs' | Sort-Object FullName) {
        $relative = $file.FullName.Substring($Root.Length + 1).Replace('\', '/')
        $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
            [IO.File]::ReadAllText($file.FullName))
        $classes = $tree.GetRoot().DescendantNodes() |
            Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax] }
        foreach ($class in $classes) {
            $name = $class.Identifier.Text
            $bases = if ($class.BaseList) {
                @($class.BaseList.Types | ForEach-Object { Get-SimpleTypeName $_.Type.ToString() })
            } else {
                @()
            }
            $fields = [Collections.Generic.List[object]]::new()
            foreach ($field in $class.Members |
                Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax] }) {
                $modifiers = $field.Modifiers.ToString()
                $attributes = @(
                    $field.AttributeLists | ForEach-Object {
                        $_.Attributes | ForEach-Object { $_.Name.ToString() }
                    }
                )
                $isPublic = $modifiers -match '(^|\s)public(\s|$)'
                $hasSerializeField = @($attributes | Where-Object {
                    $_ -match '(^|\.)SerializeField(Attribute)?$'
                }).Count -gt 0
                $hasNonSerialized = @($attributes | Where-Object {
                    $_ -match '(^|\.)NonSerialized(Attribute)?$'
                }).Count -gt 0
                $unsupportedModifier = $modifiers -match '(^|\s)(static|const|readonly)(\s|$)'
                if (!(($isPublic -or $hasSerializeField) -and !$hasNonSerialized -and !$unsupportedModifier)) {
                    continue
                }

                foreach ($variable in $field.Declaration.Variables) {
                    $fields.Add([ordered]@{
                        name = $variable.Identifier.Text
                        type = $field.Declaration.Type.ToString()
                        declaringClass = $name
                        file = $relative
                        line = $variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1
                    })
                }
            }

            if (!$map.ContainsKey($name)) {
                $map[$name] = [ordered]@{
                    name = $name
                    bases = @()
                    fields = @()
                    files = @()
                }
            }
            $map[$name].bases = @($map[$name].bases + $bases | Sort-Object -Unique)
            $map[$name].fields = @($map[$name].fields + $fields)
            $map[$name].files = @($map[$name].files + $relative | Sort-Object -Unique)
        }
    }
    return $map
}

function Test-IsUnityComponent {
    param(
        [string]$ClassName,
        $ClassMap,
        [Collections.Generic.HashSet[string]]$Seen
    )

    if (!$Seen.Add($ClassName) -or !$ClassMap.ContainsKey($ClassName)) {
        return $false
    }
    foreach ($base in $ClassMap[$ClassName].bases) {
        if ($base -in @('MonoBehaviour', 'StateMachineBehaviour', 'ScriptableObject')) {
            return $true
        }
        if (Test-IsUnityComponent $base $ClassMap $Seen) {
            return $true
        }
    }
    return $false
}

function Get-SerializedFieldsIncludingBases {
    param(
        [string]$ClassName,
        $ClassMap,
        [Collections.Generic.HashSet[string]]$Seen
    )

    if (!$Seen.Add($ClassName) -or !$ClassMap.ContainsKey($ClassName)) {
        return @()
    }
    $fields = @($ClassMap[$ClassName].fields)
    foreach ($base in $ClassMap[$ClassName].bases) {
        $fields += @(Get-SerializedFieldsIncludingBases $base $ClassMap $Seen)
    }
    return @($fields)
}

$removedClasses = @(
    'AnimStateMachine_DMD',
    'PlayAnimation',
    'GameManager',
    'PlayerInput',
    'Skill_Jiandao',
    'Weapon_Hero',
    'ZZZZZZZZZZ'
)

$resolvedRoslynRoot = Resolve-RoslynRoot $RoslynRoot
Add-Type -Path (Join-Path $resolvedRoslynRoot 'Microsoft.CodeAnalysis.dll') -ErrorAction SilentlyContinue
Add-Type -Path (Join-Path $resolvedRoslynRoot 'Microsoft.CodeAnalysis.CSharp.dll') -ErrorAction SilentlyContinue

$sourceRoot = Join-Path $SourceProject 'Assets/Game/Scripts'
$targetRoot = Join-Path $TargetProject 'Assets/Game/Scripts/ColorTiming'
if (!(Test-Path -LiteralPath $sourceRoot)) {
    throw "Source script root does not exist: $sourceRoot"
}
if (!(Test-Path -LiteralPath $targetRoot)) {
    throw "Target script root does not exist: $targetRoot"
}

$sourceClasses = Build-ClassMap $sourceRoot
$targetClasses = Build-ClassMap $targetRoot
$records = [Collections.Generic.List[object]]::new()
foreach ($className in $sourceClasses.Keys | Sort-Object) {
    if ($removedClasses -contains $className) {
        continue
    }
    if (!(Test-IsUnityComponent $className $sourceClasses ([Collections.Generic.HashSet[string]]::new()))) {
        continue
    }

    $sourceFields = @(Get-SerializedFieldsIncludingBases $className $sourceClasses (
        [Collections.Generic.HashSet[string]]::new()))
    $targetFields = if ($targetClasses.ContainsKey($className)) {
        @(Get-SerializedFieldsIncludingBases $className $targetClasses (
            [Collections.Generic.HashSet[string]]::new()))
    } else {
        @()
    }
    $missingFields = @(
        $sourceFields | Where-Object {
            $sourceField = $_
            @($targetFields | Where-Object { $_.name -eq $sourceField.name }).Count -eq 0
        }
    )
    $typeChanges = [Collections.Generic.List[object]]::new()
    foreach ($sourceField in $sourceFields) {
        $targetField = @($targetFields | Where-Object { $_.name -eq $sourceField.name } |
            Select-Object -First 1)
        if ($targetField.Count -gt 0 -and $targetField[0].type -ne $sourceField.type) {
            $typeChanges.Add([ordered]@{
                name = $sourceField.name
                sourceType = $sourceField.type
                targetType = $targetField[0].type
            })
        }
    }

    $records.Add([ordered]@{
        class = $className
        sourceFiles = $sourceClasses[$className].files
        targetFiles = if ($targetClasses.ContainsKey($className)) {
            $targetClasses[$className].files
        } else {
            @()
        }
        sourceFieldCount = $sourceFields.Count
        targetFieldCount = $targetFields.Count
        sourceFields = $sourceFields
        missingFields = $missingFields
        typeChanges = $typeChanges
    })
}

$missingClasses = @($records | Where-Object { $_.targetFiles.Count -eq 0 })
$missingFieldRecords = @($records | Where-Object { $_.missingFields.Count -gt 0 })
$typeChangeRecords = @($records | Where-Object { $_.typeChanges.Count -gt 0 })
$componentCountMatches = $records.Count -eq 54
$fieldContractCount = @($records | ForEach-Object { $_.sourceFields }).Count
$fieldCountMatches = $fieldContractCount -eq 241
$passed = $componentCountMatches -and $fieldCountMatches -and
    $missingClasses.Count -eq 0 -and $missingFieldRecords.Count -eq 0 -and
    $typeChangeRecords.Count -eq 0

$report = [ordered]@{
    status = if ($passed) { 'PASS' } else { 'FAIL' }
    generatedAtUtc = [DateTime]::UtcNow.ToString('o')
    sourceProject = (Resolve-Path -LiteralPath $SourceProject).Path
    targetProject = (Resolve-Path -LiteralPath $TargetProject).Path
    roslynRoot = $resolvedRoslynRoot
    sourceComponentClassCount = $records.Count
    sourceSerializedFieldContractCount = $fieldContractCount
    missingTargetClassCount = $missingClasses.Count
    missingSerializedFieldCount = @($missingFieldRecords | ForEach-Object { $_.missingFields }).Count
    serializedFieldTypeChangeCount = @($typeChangeRecords | ForEach-Object { $_.typeChanges }).Count
    countChecks = [ordered]@{
        componentClasses54 = $componentCountMatches
        inheritedFieldContracts241 = $fieldCountMatches
    }
    failures = @($records | Where-Object {
        $_.targetFiles.Count -eq 0 -or $_.missingFields.Count -gt 0 -or $_.typeChanges.Count -gt 0
    })
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
$report | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $resolvedOutput -Encoding utf8
[pscustomobject]$report | Select-Object status, sourceComponentClassCount,
    sourceSerializedFieldContractCount, missingTargetClassCount,
    missingSerializedFieldCount, serializedFieldTypeChangeCount | Format-List

if (!$passed) {
    exit 1
}
