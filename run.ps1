$svnUrl = Read-Host "Enter URL of SVN repository"
$startRevision = Read-Host "Enter start revision"
$endRevision = Read-Host "Enter end revision"
$destinationFolder = Read-Host "Enter destination folder"
$svnUser = Read-Host "Enter SVN user name"
$svnPwd = Read-Host "Enter SVN password"

$exe = "src\Uncas.SvnTools.ExportChanges\bin\Release\Uncas.SvnTools.ExportChanges.exe"
msbuild Uncas.SvnTools.sln /p:Configuration=Release
if (Test-Path $destinationFolder) { rmdir -force -recurse $destinationFolder }
. $exe $svnUrl $startRevision $endRevision $destinationFolder $svnUser $svnPwd