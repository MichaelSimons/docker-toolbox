[cmdletbinding()]
param(
    [string]$Repo,
    [string]$PlatformTag,
    [string]$Tags,
    [string]$Username,
    [string]$Password
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Exec($cmd) {
    Write-Host -foregroundcolor Cyan "$(hostname) > $cmd $args"
    & $cmd @args
    if ($LastExitCode -ne 0) {
        fatal 'Command exited with non-zero code'
    }
}

$Tags -split ", " |
    %{
        Write-Host "Generating manifest for ${Repo}:${_}"
        (Get-Content manifest-template.yml).Replace("{repo}", $Repo).Replace("{platformTag}", $PlatformTag).Replace("{tag}", $_) `
            | Out-File manifest.yml
        $temp = Get-Content manifest.yml
        Write-Host $temp
        Exec docker run --rm -v /var/run/docker.sock:/var/run/docker.sock -v ${pwd}:/manifests manifest-tool --username $Username --password $Password push from-spec /manifests/manifest.yml
        Remove-Item manifest.yml
    }