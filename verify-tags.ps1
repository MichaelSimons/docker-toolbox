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
$ErrorActionPreference="Stop"

function RetrieveTagInfo() {
# TODO - Option to append non-documented tags to readme tags

    if ($Readme) {
        $tagInfo = @{}
        type $Readme |
            where { $_ -match "^\s*-\s*\[(?<tags>``[-._:a-z0-9]+``(?:,\s*``[-._:a-z0-9]+``)*)\s\(\*(?<alias>.+)\*\)\]" } |
            where { ($Platform -eq "win" -and $Matches["alias"].Contains("nano")) -or `
                ($Platform -eq "linux" -and !($Matches["alias"].Contains("nano"))) } |
            % { $tagInfo.Add($Matches["alias"], $Matches["tags"].Replace('`', "").Split(",").Trim()) }
    }
    else {
        $tagInfo = $Tags
    }

# TODO: dump tagInfo
#$tagInfo | Format-Table -AutoSize -Expand Both

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
        $info = (docker inspect "${Repo}:$($tags[0])" |
            ConvertFrom-Json)[0]
# TODO - handle not found

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
# TODO - format table
            foreach ($tag in $imageVariant.Value) {
# TODO - error gets written to output on error
                $inspectResult = docker inspect "${Repo}:$tag"
                if (-NOT $?) {
                    Write-Host "$tag - Not Found" -ForegroundColor Red
                    continue
                }

                $info = ($inspectResult | ConvertFrom-Json)[0]
                Write-Host "$tag - $($info.Id)" -ForegroundColor Red
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
