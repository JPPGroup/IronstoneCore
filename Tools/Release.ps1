$scriptLocation = ($PSScriptRoot + '\TestRunner\AutoNetLoadOriginal.scr')
$modifiedScriptLocation = ($PSScriptRoot + '\TestRunner\AutoNetLoad.scr')
$text = (Get-Content -Path $scriptLocation -ReadCount 0) -join "`n"	
$replace = ($PSScriptRoot)
$newtext = $text -replace '{dir}', $replace
Set-Content -Path $modifiedScriptLocation -Value $newtext

$testDir = ($PSScriptRoot + "\..\bin\x64\Release\")

& "C:\Program Files\Autodesk\AutoCAD 2019\accoreconsole.exe" /s ($PSScriptRoot + "\TestRunner\AutoNetLoad.scr")  /tests:"$testDir" /isolate

Write-Host "";
Write-Host "";
Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
