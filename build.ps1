[CmdletBinding()]
Param 
(
	[Parameter(Mandatory = $true)][string]$runtime
)

echo $env:APPVEYOR_BUILD_VERSION
echo $env:Configuration
echo $runtime

& ./clean.ps1
dotnet publish -r $runtime -c Debug /p:PublishSingleFile=true /p:PublishTrimmed=true --output publish/$runtime src/AmqpTools/AmqpTools.csproj

ls publish/$runtime
