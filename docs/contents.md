# Contents

## show full csproj file

```dotnet build -pp > full.xml```

## runtimeconfig.json shared frameworks and frameworks reference

## dotnet publish

```dotnet publish -o ./publish -pp full.xml```

* -P:PublishReadyToRun
* -p:PublishSingleFile=true
* -p:PublishTrimmed=true
* -r linux-x64 win-x64 osx-x64

## dotnet tools
dotnet tool install -g Microsoft.dotnet-httprepl


## Extensions: Hosting, Configuration, Logging and Dependency Injection

* dotnet new worker
* dotnet new web
* dotnet new webapi


curl https://localhost:5001/api/meetup/events --request POST --data '{"title":"event driven architectures", "capacity": 10}' --header 'Content-Type: application/json'