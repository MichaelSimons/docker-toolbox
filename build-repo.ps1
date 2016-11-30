[cmdletbinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$DockerRepo,
    [Parameter(Mandatory=$true)]
    [ValidateSet("win", "linux")]
    [string]$Platform,
    [string]$DockerfileName,
    [switch]$UseImageCache
)

Set-StrictMode -Version Latest
$ErrorActionPreference="Stop"

$dirSeparator = [IO.Path]::DirectorySeparatorChar

if ($UseImageCache) {
    $optionalDockerBuildArgs = ""
}
else {
    $optionalDockerBuildArgs = "--no-cache"
}

if (!Dockerfile) {
    if ($Platform -eq "win") {
        $DockerfileName = "Dockerfile.nano"
    }
    else {
        $DockerfileName = "Dockerfile"
    }
}

Get-ChildItem -Recurse -Filter $DockerfileName |
    sort DirectoryName |
    foreach {
        $tag = "$($DockerRepo):" + 
            $_.DirectoryName.
            Replace("$pwd$dirSeparator", '').
            Replace($dirSeparator, '-')

        Write-Host "--- Building $tag from $($_.DirectoryName) ---"
        docker build $optionalDockerBuildArgs -t $tag $_.DirectoryName
        if (-NOT $?) {
            throw "Failed building $tag"
        }
    }

popd
