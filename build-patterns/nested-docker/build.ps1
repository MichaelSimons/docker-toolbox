Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Exec([scriptblock]$cmd, [string]$errorMessage = "Error executing command: " + $cmd) {
    Write-Host "Executing: $cmd"
    & $cmd
    if ($LastExitCode -ne 0) {
        throw $errorMessage
    }
}

exec { docker build -t build-image -f Dockerfile.build . }
exec { docker run --rm -v //var/run/docker.sock:/var/run/docker.sock build-image docker build -t dotnetbot:nested-docker . }
