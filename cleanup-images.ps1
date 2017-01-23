[cmdletbinding()]
param(
    [string]$Repo,
    [switch]$SkipContainerCleanup
)

# consider SkipBaseImages option - use docker inspect to find parentless images

if (!$SkipContainerCleanup) {
    docker ps -a -q | %{docker rm -f $_}
}

docker images |
    where {(!$_.StartsWith("IMAGE "))}
    where {(!$Repo) -or ($_.StartsWith("$Repo "))} |
    %{$_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)[2]} |
    select-object -unique |
    %{docker rmi -f $_}