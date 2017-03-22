Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Exec([scriptblock]$cmd, [string]$errorMessage = "Error executing command: " + $cmd) {
    Write-Host "Executing: $cmd"
    & $cmd
    if ($LastExitCode -ne 0) {
        throw $errorMessage
    }
}

exec { docker run --rm -v ${PWD}:/dotnetbot -w /dotnetbot microsoft/dotnet:1.0-sdk dotnet restore }
exec { docker run --rm -v ${PWD}:/dotnetbot -w /dotnetbot microsoft/dotnet:1.0-sdk dotnet publish -c Release -o out }
exec { docker build -t dotnetbot:src-volume . }