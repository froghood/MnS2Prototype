Remove-Item -r -fo ./bin/release;
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=True -p:PublishTrimmed=False -p:TrimMode=CopyUsed -p:PublishReadyToRun=True;
if ($?) {
    Copy-Item -path ./assets -d ./bin/release/net7.0/win-x64/publish -r
    Copy-Item ./Settings.json -d ./bin/release/net7.0/win-x64/publish
    Copy-Item ./KeyConfig.json -d ./bin/release/net7.0/win-x64/publish
}