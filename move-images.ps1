<#
.DESCRIPTION
Moves images between repos.
#>

[cmdletbinding()]
param(
    [string]$SourceRepo,
    [string]$DestRepo
)

docker images |
    %{
        $info = $_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
        "$($info[0]):$($info[1])"
    } |
    where {($_.StartsWith("$($SourceRepo):"))} |
    %{
        $newTag = $_.Replace("$($SourceRepo):", "$($DestRepo):")
        Write-Host "docker tag $_ $newTag"
        docker tag $_ $newTag
    }