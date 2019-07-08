param($installPath, $toolsPath, $package, $project)
$targetPath = "$installPath\..\..\buildeploy"

if (-Not (Test-Path $targetPath  -PathType Container))
{
    md $targetPath    
}

copy "$toolspath\*.dll" $targetPath
copy "$toolspath\lib\*.*" $targetPath -Recurse
copy "$toolspath\*.targets" $targetPath
