[CmdletBinding()]
Param 
(
	[Parameter(Mandatory = $true)][string]$runtime
)

echo $env:APPVEYOR_BUILD_VERSION
echo $env:Configuration
echo $runtime

& ./clean.ps1
#dotnet build -r $runtime src/AmqpShovel/AmqpShovel.csproj --configuration $env:Configuration /property:Version=$env:APPVEYOR_BUILD_VERSION
#dotnet build -r $runtime src/AmqpPublisher/AmqpPublisher.csproj --configuration $env:Configuration /property:Version=$env:APPVEYOR_BUILD_VERSION
#dotnet build -r $runtime src/AmqpQueue/AmqpQueue.csproj --configuration $env:Configuration /property:Version=$env:APPVEYOR_BUILD_VERSION
dotnet publish -r $runtime -c Debug /p:PublishSingleFile=true /p:PublishTrimmed=true --output publish/$runtime src/AmqpShovel/AmqpShovel.csproj
dotnet publish -r $runtime -c Debug /p:PublishSingleFile=true /p:PublishTrimmed=true --output publish/$runtime src/AmqpPublisher/AmqpPublisher.csproj
dotnet publish -r $runtime -c Debug /p:PublishSingleFile=true /p:PublishTrimmed=true --output publish/$runtime src/AmqpQueue/AmqpQueue.csproj

ls publish/$runtime
