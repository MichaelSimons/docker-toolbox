[cmdletbinding()]
param(
    [string]$Repo,
    [string]$Tags
)

$Tags -split ", " |
    %{
        Write-Host "docker pull $Repo/$_"
        docker pull ${Repo}:${_}
    }