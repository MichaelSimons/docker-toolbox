<#
.DESCRIPTION
Pushes images all of the images from a given repo
#>

[cmdletbinding()]
param(
    [string]$Repo
)

docker images |
    %{
        $info = $_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
        "$($info[0]):$($info[1])"
    } |
    where {($_.StartsWith("$($Repo):"))} |
    %{
        Write-Host "docker push $_"
        docker push $_
    }