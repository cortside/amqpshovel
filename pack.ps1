[CmdletBinding()]
Param 
(
    [Parameter(Mandatory = $true)][string]$version
)

dotnet pack src --include-symbols -p:SymbolPackageFormat=snupkg --configuration Debug -o ((get-location).Path + '\artifacts') /property:Version=$version