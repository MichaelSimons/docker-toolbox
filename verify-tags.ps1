[cmdletbinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$Repo,
    [switch]$UseLocalImages,
    [Parameter(ParameterSetName='ParseReadme', Mandatory)]
    [string]$Readme,
    [Parameter(ParameterSetName='ParseReadme', Mandatory)]
    [ValidateSet("win", "linux")]
    [string]$Platform,
    [Parameter(ParameterSetName='SpecifiedTags', Mandatory)]
    [hashtable]$Tags
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function RetrieveTagInfo() {
# TODO - Option to append undocumented tags to readme tags
# TODO - Option to use alias instead of link

    if ($Readme) {
        $tagInfo = @{}
        type $Readme |
            where { $_ -match "^\s*-\s*\[(?<tags>``[-._:a-z0-9]+``(?:,\s*``[-._:a-z0-9]+``)*)\s\(\*(?<alias>.+)\*\)\]\((?<link>.+)\)" } |
            where { ($Platform -eq "win" -and $Matches["tags"].Contains("-nano")) -or `
                ($Platform -eq "linux" -and !($Matches["tags"].Contains("-nano"))) } |
            % { $tagInfo.Add($Matches["link"], $Matches["tags"].Replace('`', "").Split(",").Trim()) }
    }
    else {
        $tagInfo = $Tags
    }

    $formatExpressions = @{Expression={$_.Name};Label="Dockerfile"}, @{Expression={$_.Value};Label="Tags"}
    $tagInfo | Format-Table -AutoSize $formatExpressions | Out-String | Write-Host

    return $tagInfo
}

function PullTags ([Hashtable] $TagInfo) {
    foreach ($imageVariant in $TagInfo.GetEnumerator()) {
        $logMsg = "Pulling $($imageVariant.Name) tags"
        Write-Host $logMsg
        $logMsg = "-" * $logMsg.Length
        Write-Host $logMsg

        foreach ($tag in $imageVariant.Value) {
            docker pull "${Repo}:${tag}"
            Write-Host "`n"
        }

        Write-Host "`n"
    }
}

function VerifyTags ([Hashtable] $TagInfo) {
    foreach ($imageVariant in $TagInfo.GetEnumerator()) {
        $logMsg = "Verifing $($imageVariant.Name) tags"
        Write-Host $logMsg
        $logMsg = "-" * $logMsg.Length
        Write-Host $logMsg

        $tags = [string[]]($imageVariant.Value)
        $inspectResult = docker inspect "${Repo}:$($tags[0])"
        if (-NOT $?) {
            Write-Host "$($tags[0]) - Not Found`n" -ForegroundColor Red
            continue
        }

        $info = ($inspectResult | ConvertFrom-Json)[0]
        $repotags = [string[]]($info.RepoTags |
            %{$_.Substring($Repo.Length + 1)})

        $result = ($repoTags.Count -eq $tags.Count)
        if ($result) {
            foreach ($tag in $tags) {
                if ($repoTags -notcontains $tag)
                {
                    result = $false
                    break
                }
            }
        }

        if (!$result) {
            Write-Host "The following tags do not reference the same image:" `
                -ForegroundColor Red
            foreach ($tag in $imageVariant.Value) {
                $inspectResult = docker inspect "${Repo}:$tag"
                if (-NOT $?) {
                    $message = "Not Found"
                }
                else {
                    $info = ($inspectResult | ConvertFrom-Json)[0]
                    $message = $info.Id
                }

                Write-Host ("{0,-36} {1}" -f $tag, $message) -ForegroundColor Red
            }
        }
        else {
            Write-Host "The following tags reference the same image ($($info.Id)): $($imageVariant.Value -join ', ')" `
                -ForegroundColor Green
        }

        Write-Host "`n"
    }
}

$tagInfo = RetrieveTagInfo

if (!$UseLocalImages) {
    PullTags -TagInfo $tagInfo
}

VerifyTags -TagInfo $tagInfo

# TODO - Verify derived images - runtime from runtime-deps
