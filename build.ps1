
$buildPath = "./bin/release/net7.0/win-x64/publish"

Remove-Item -r -fo ./bin/release;
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=True -p:PublishTrimmed=False -p:TrimMode=CopyUsed -p:PublishReadyToRun=True;
if ($?) {
    Copy-Item -path ./assets -Destination $buildPath -Recurse
    Copy-Item ./Settings.json -Destination $buildPath
    Copy-Item ./KeyConfig.json -Destination $buildPath
    Copy-Item "./steam_api64.dll" -Destination $buildPath
    
    # copy to vbox folder
    # Copy-Item -Path "./bin/release/net7.0/win-x64/publish/*" -Destination "./vboxshared" -Recurse -Force
    
    # archive using 7zip
    $destination = "./bin/release/net7.0/win-x64/publish/" + $args[0] + ".7z"
    $files = (Get-ChildItem -Path $buildPath -Name).ForEach({ $buildPath + "/" + $_ })
    
    & "C:\Program Files\7-Zip\7z.exe" a $destination $files
    
    # upload to google drive using sharex
    $destinationFullPath = Resolve-Path -Path $destination
    
    Write-Output Uploading...
    & "C:\Program Files\ShareX\ShareX.exe" $destinationFullPath -task "Upload to GDrive"
}