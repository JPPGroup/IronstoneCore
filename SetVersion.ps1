$version = git describe --tags --always
$charCount = $version.Length - $version.IndexOf("-")
$preverison = $version.Substring($version.Length - $charCount)
$build = $args[0]
Write-Host "##teamcity[setParameter name='TagVersion' value='$version']"
Write-Host "##teamcity[setParameter name='TagPreVersion' value='$preverison']"

$files = Get-ChildItem -Include *AssemblyInfo.cs -Recurse 
foreach ($file in $files){
	$text = (Get-Content -Path $file -ReadCount 0) -join "`n"	
    $replace = ('$1"' + $version + '.' + $build + '"$3')
    Write-Host $replace
	$text = $text -replace '(\[assembly\: AssemblyVersion\()\"(\d\.\d\.\d\.\d)\"(\)\])', $replace	
    $text = $text -replace '(\[assembly\: AssemblyFileVersion\()\"(\d\.\d\.\d\.\d)\"(\)\])', $replace
	Set-Content -Path $file -Value $text	
}

$updatefile = Get-ChildItem -Include *IronstoneCore*.xml -Recurse 
foreach ($file in $updatefile){
	$text = (Get-Content -Path $file -ReadCount 0) -join "`n"	
    $replace = ($version + '.' + $build)
    Write-Host $replace
	$text = $text -replace '{version}', $replace	
	Set-Content -Path $file -Value $text	
}

Write-Host "Version set to $version.$build"