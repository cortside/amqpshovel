echo $env:APPVEYOR_BUILD_VERSION
echo $env:Configuration

dotnet build src/AmqpTools.sln --configuration $env:Configuration /property:Version=$env:APPVEYOR_BUILD_VERSION
dotnet publish -r win-x64 -c Debug /p:PublishSingleFile=true --self-contained --output publish/win-x64 src/AmqpShovel/AmqpShovel.csproj
dotnet publish -r win-x64 -c Debug /p:PublishSingleFile=true --self-contained --output publish/win-x64 src/AmqpPublisher/AmqpPublisher.csproj
dotnet publish -r win-x64 -c Debug /p:PublishSingleFile=true --self-contained --output publish/win-x64 src/AmqpQueue/AmqpQueue.csproj
dotnet build src/AmqpTools.sln --configuration $env:Configuration /property:Version=$env:APPVEYOR_BUILD_VERSION
dotnet publish -r alpine-x64 -c Debug /p:PublishSingleFile=true --self-contained --output publish/alpine-x64 src/AmqpShovel/AmqpShovel.csproj
dotnet publish -r alpine-x64 -c Debug /p:PublishSingleFile=true --self-contained --output publish/alpine-x64 src/AmqpPublisher/AmqpPublisher.csproj
dotnet publish -r alpine-x64 -c Debug /p:PublishSingleFile=true --self-contained --output publish/alpine-x64 src/AmqpQueue/AmqpQueue.csproj