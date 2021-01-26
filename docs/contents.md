# Contents


## show full csproj file

```dotnet build -pp > full.xml```

## dotnet publish

```dotnet publish -o ./publish```

* -P:PublishReadyToRun
* -p:PublishSingleFile=true
* -p:PublishTrimmed=true
* -r linux-x64 win-x64 osx-x64

## dotnet tools

dotnet tool install -g Microsoft.dotnet-httprepl

