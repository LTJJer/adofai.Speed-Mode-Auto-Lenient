<#
  Static self-check: loads the built assembly and verifies that Harmony patch targets
  and the game-side members it relies on still exist with the expected signatures.

  Usage:
    powershell -File tools\verify.ps1
    powershell -File tools\verify.ps1 -Managed "F:\...\A Dance of Fire and Ice_Data\Managed"
#>
param(
    [string]$Managed = $env:ADOFAI_MANAGED
)

$ErrorActionPreference = 'Stop'

if (-not $Managed) {
    $Managed = 'F:\SteamLibrary\steamapps\common\A Dance of Fire and Ice\A Dance of Fire and Ice_Data\Managed'
}

$root   = Split-Path -Parent $PSScriptRoot
$modDll = Join-Path $root 'dist\ADOFAI.SpeedModeAutoLenient\ADOFAI.SpeedModeAutoLenient.dll'

if (-not (Test-Path $modDll)) {
    throw "Build output not found: $modDll (run 'dotnet build -c Release' first)"
}

$probe = @($Managed, (Join-Path $Managed 'UnityModManager'))
[System.AppDomain]::CurrentDomain.add_AssemblyResolve([System.ResolveEventHandler]{
    param($s, $e)
    $simple = ($e.Name -split ',')[0]
    foreach ($dir in $script:probe) {
        $p = Join-Path $dir ($simple + '.dll')
        if (Test-Path $p) { return [System.Reflection.Assembly]::LoadFrom($p) }
    }
    return $null
})

Write-Output "=== Load Assembly-CSharp ==="
$ac = [System.Reflection.Assembly]::LoadFrom((Join-Path $Managed 'Assembly-CSharp.dll'))

Write-Output "=== Load mod ==="
$mod = [System.Reflection.Assembly]::LoadFrom($modDll)
Write-Output ("  " + $mod.GetName().FullName)
Write-Output ("  TargetFramework: " + ($mod.GetCustomAttributes([System.Runtime.Versioning.TargetFrameworkAttribute], $false) | ForEach-Object { $_.FrameworkName }))

Write-Output "=== Referenced assemblies ==="
foreach ($r in $mod.GetReferencedAssemblies()) {
    $found = $false
    foreach ($dir in $probe) {
        if (Test-Path (Join-Path $dir ($r.Name + '.dll'))) { $found = $true; break }
    }
    Write-Output ("  {0,-36} {1}" -f $r.Name, $found)
}

Write-Output "=== Types ==="
foreach ($t in $mod.GetTypes()) {
    Write-Output ("  " + $t.FullName)
}

Write-Output "=== EntryMethod ==="
$main = $mod.GetType('ADOFAI.SpeedModeAutoLenient.Main')
$load = $main.GetMethod('Load', [System.Reflection.BindingFlags]'Public,Static')
Write-Output ("  Main.Load -> " + $load.ReturnType.Name + "(" + (($load.GetParameters() | ForEach-Object { $_.ParameterType.FullName }) -join ', ') + ")")

Write-Output "=== Patch targets ==="
$editor = $ac.GetType('scnEditor')
$mReg = $editor.GetMethod('RegisterKeybinds', [System.Reflection.BindingFlags]'Instance,NonPublic')
$mSwitch = $editor.GetMethod('SwitchToEditMode', [System.Reflection.BindingFlags]'Instance,Public')
$fKb = $editor.GetField('keybindManager', [System.Reflection.BindingFlags]'Instance,NonPublic')
$pFloors = $editor.GetProperty('floors', [System.Reflection.BindingFlags]'Instance,Public')
Write-Output ("  scnEditor.RegisterKeybinds (private): " + ($null -ne $mReg))
Write-Output ("  scnEditor.SwitchToEditMode (bool):    " + ($null -ne $mSwitch))
Write-Output ("  field keybindManager:                 " + ($null -ne $fKb) + " (" + $fKb.FieldType.FullName + ")")
Write-Output ("  property floors:                      " + ($null -ne $pFloors))

Write-Output "=== Game-side members ==="
$gcs = $ac.GetType('GCS')
$fDiff = $gcs.GetField('difficulty', [System.Reflection.BindingFlags]'Public,Static')
Write-Output ("  GCS.difficulty: " + ($null -ne $fDiff) + " (" + $fDiff.FieldType.FullName + ")")
Write-Output ("  Difficulty values: " + (([Enum]::GetNames($ac.GetType('Difficulty'))) -join ', '))
$mgrType = $ac.GetType('ADOFAI.Editor.EditorKeybindManager')
Write-Output ("  EditorKeybindManager.dictionary: " + ($null -ne $mgrType.GetProperty('dictionary')))
Write-Output ("  EditorKeybindManager.RegisterKeybind: " + ($null -ne $mgrType.GetMethod('RegisterKeybind')))
$actionType = $mod.GetType('ADOFAI.SpeedModeAutoLenient.PlayWithAutoLenientEditorAction')
Write-Output ("  PlayWithAutoLenientEditorAction base:  " + $actionType.BaseType.FullName)

Write-Output "DONE"
