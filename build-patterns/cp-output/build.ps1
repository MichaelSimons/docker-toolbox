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
exec { docker create --name build-container build-image }
try {
    exec { docker cp build-container:/dotnetbot/out ./ }
}
finally {
    exec { docker rm build-container }
}
exec { docker build -t dotnetbot:cp-src . }