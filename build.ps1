[CmdletBinding()]
Param 
(
	[Parameter(Mandatory = $true)][string]$runtime
)

echo $env:APPVEYOR_BUILD_VERSION
echo $env:Configuration
echo $runtime

& ./clean.ps1
dotnet build src/AmqpTools.sln --configuration $env:Configuration /property:Version=$env:APPVEYOR_BUILD_VERSION
dotnet publish -r $runtime -c Debug /p:PublishSingleFile=true --output publish/$runtime src/AmqpShovel/AmqpShovel.csproj
dotnet publish -r $runtime -c Debug /p:PublishSingleFile=true --output publish/$runtime src/AmqpPublisher/AmqpPublisher.csproj
dotnet publish -r $runtime -c Debug /p:PublishSingleFile=true --output publish/$runtime src/AmqpQueue/AmqpQueue.csproj
