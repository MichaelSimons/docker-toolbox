[cmdletbinding()]
param(
    [string]$RepoPrefix,
    [switch]$SkipContainerCleanup
)

# consider SkipBaseImages option - use docker inspect to find parentless images

if (!$SkipContainerCleanup) {
    docker ps -a -q | %{docker rm -f $_}
}

docker images |
    where {(!$RepoPrefix) -or ($_.StartsWith("$RepoPrefix"))} |
    %{$_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)[2]} |
    where {(!$_.StartsWith("IMAGE"))} |
    select-object -unique |
    %{docker rmi -f $_}