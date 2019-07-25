function Log {
    param ([string] $Message)

    Write-Output $Message
}

function Exec {
    param ([string] $Cmd)

    Log "Executing: '$Cmd'"
    Invoke-Expression $Cmd
    if ($LASTEXITCODE -ne 0) {
        throw "Failed: '$Cmd'"
    }
}

function PullImages {
    param ([string] $Repo1, [string] $Repo2, [string] $Tag)

    Exec "docker pull ${Repo1}:$Tag"
    Exec "docker pull ${Repo2}:$Tag"
    Log "---------------------------------------------------------"
}

function CompareIds {
    param ([string] $Repo1, [string] $Repo2, [string] $Tag)

    $repo1Id = docker inspect --format="{{.Id}}" "${Repo1}:$Tag"
    $repo2Id = docker inspect --format="{{.Id}}" "${Repo2}:$Tag"
    if ($repo1Id -ne $repo2Id)
    {
        Write-Error "'${Repo1}:$Tag' and '${Repo2}:$Tag' are not equivalent"
        Exec "docker history ${Repo1}:$Tag"
        Exec "docker history ${Repo2}:$Tag"
    }
    else 
    {
        Write-Output "'${Repo1}:$Tag' and '${Repo2}:$Tag' are equivalent"
        Exec "docker history ${Repo1}:$Tag"
    }
    Log "---------------------------------------------------------"
}

function CompareImages {
    param ([string] $Repo1, [string] $Repo2, [string[]] $Tags)

    $Tags |
        ForEach-Object { PullImages -Repo1 $Repo1 -Repo2 $Repo2 -Tag $_ }

    $Tags |
        ForEach-Object { CompareIds -Repo1 $Repo1 -Repo2 $Repo2 -Tag $_}
}

$nanoTags = @("sac2016", "1803")
$serverCoreTags = @("ltsc2016", "1803")

# CompareImages -Repo1 "microsoft/nanoserver" -Repo2 "mcr.microsoft.com/windows/nanoserver" -Tags $nanoTags

# CompareImages -Repo1 "mcr.microsoft.com/windows/servercore" -Repo2 "microsoft/windowsservercore" -Tags $serverCoreTags

Exec "docker pull mcr.microsoft.com/windows/servercore:ltsc2016"
Exec "docker history mcr.microsoft.com/windows/servercore:ltsc2016"
Exec "docker pull mcr.microsoft.com/windows/nanoserver:1803"
Exec "docker history mcr.microsoft.com/windows/nanoserver:1803"
Exec "docker pull mcr.microsoft.com/windows/servercore:1803"
Exec "docker history mcr.microsoft.com/windows/servercore:1803"
Exec "docker pull mcr.microsoft.com/windows/nanoserver:1809"
Exec "docker history mcr.microsoft.com/windows/nanoserver:1809"
Exec "docker pull mcr.microsoft.com/windows/servercore:1809"
Exec "docker history mcr.microsoft.com/windows/servercore:1809"
Exec "docker pull mcr.microsoft.com/windows/servercore:ltsc2019"
Exec "docker history mcr.microsoft.com/windows/servercore:ltsc2019"
Exec "docker pull mcr.microsoft.com/windows/nanoserver:1809-arm32v7"
Exec "docker history mcr.microsoft.com/windows/nanoserver:1809-arm32v7"
Exec "docker pull mcr.microsoft.com/windows/nanoserver:1903"
Exec "docker history mcr.microsoft.com/windows/nanoserver:1903"
Exec "docker pull mcr.microsoft.com/windows/servercore:1903"
Exec "docker history mcr.microsoft.com/windows/servercore:1903"
