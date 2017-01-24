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
    where {(!$Repo) -or ($_.StartsWith("$Repo "))} |
    %{$_.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)[2]} |
    where {(!$_.StartsWith("IMAGE"))} |
    select-object -unique |
    %{docker rmi -f $_}