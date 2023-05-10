rm -r -fo ./bin/release;
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=True -p:PublishTrimmed=True -p:TrimMode=CopyUsed -p:PublishReadyToRun=True;
if ($?) {
    copy -path ./assets -d ./bin/release/net6.0/win-x64/publish -r
    copy ./Settings.json -d ./bin/release/net6.0/win-x64/publish
    copy ./KeyConfig.json -d ./bin/release/net6.0/win-x64/publish
}