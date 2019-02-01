$version = git describe --tags --always
$build = $args[0]
Write-Host "##teamcity[setParameter name='TagVersion' value='$version']"

$files = Get-ChildItem -Include *AssemblyInfo.cs -Recurse 
foreach ($file in $files){
	$text = (Get-Content -Path $file -ReadCount 0) -join "`n"	
    $replace = ('$1"' + $version + '.' + $build + '"$3')
    Write-Host $replace
	$text = $text -replace '(\[assembly\: AssemblyVersion\()\"(\d\.\d\.\d\.\d)\"(\)\])', $replace	
    $text = $text -replace '(\[assembly\: AssemblyFileVersion\()\"(\d\.\d\.\d\.\d)\"(\)\])', $replace
	Set-Content -Path $file -Value $text	
}