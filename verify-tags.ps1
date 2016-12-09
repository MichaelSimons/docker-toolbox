<#
.DESCRIPTION
Verifies the various image tags of the specified Docker repository.  This tool verifies that the various tags for a
specified image point to the same image.  The tool also verifies the images reference the latest FROM image that is available.

.PARAMETER Repo
The Docker repository to operate on.

.PARAMETER UseLocalImages
Don't pull the latest images and use the images that exist locally.

.PARAMETER Readme
The path to the readme for the Docker repository.  The tags to verify will be extracted from this readme.

.PARAMETER Platform
The Docker host OS platform to verify the images for.

.PARAMETER Tags
A Hashtable of the tags to verify.  The Key of each entry is the path to the Dockerfile to verify.  The Value of each
entry is an array of the tags of the Dockerfile to verify.

.EXAMPLE
.\verify-tags.ps1 -Repo microsoft/dotnet -Readme \dotnet-docker\1.0\README.md -Platform linux
#>
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

function VerifyTagEquivalence ([Hashtable] $TagInfo) {
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
        $repoTags = [string[]]($info.RepoTags |
            %{$_.Substring($Repo.Length + 1)})

        $result = ($RepoTags.Count -eq $Tags.Count)
        if ($result) {
            foreach ($tag in $Tags) {
                if ($RepoTags -notcontains $tag)
                {
                    result = $false
                    break
                }
            }
        }

        if (!$result) {
            Write-Host "The following tags do not reference the same image:" `
                -ForegroundColor Red
            foreach ($tag in $Tags) {
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
            Write-Host "The following tags reference the same image ($($info.Id)): $($Tags -join ', ')" `
                -ForegroundColor Green
        }

        Write-Host "`n"
    }
}

function VerifyFrom ([Hashtable] $TagInfo)
{
# TODO - Consider tracking images already pulled so as to not attempting to repull them continuously for small perf gain
    foreach ($imageVariant in $TagInfo.GetEnumerator()) {
        $logMsg = "Verifing $($imageVariant.Name) FROM layers"
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
        $dockerfile = Invoke-WebRequest $imageVariant.Name.Replace("github.com", "raw.githubusercontent.com").Replace("/blob", "")
        $fromMatch = $dockerfile -match "FROM (?<fromImage>.+)"
        $from = $Matches["fromImage"]
        Write-Host "Base Image: $from"

        if (!$UseLocalImages) {
            docker pull $from
        }

        $fromInspectResult = docker inspect $from
        if (-NOT $?) {
            Write-Host "$from - Not Found`n" -ForegroundColor Red
            continue
        }

        $fromInfo = ($fromInspectResult | ConvertFrom-Json)[0]

        For ($i=0; $i -le ($fromInfo.RootFS.Layers.Count - 1); $i++) {
            if ($fromInfo.RootFS.Layers[$i] -eq $info.RootFS.Layers[$i]) {
                Write-Host "Equivalent base layer $($fromInfo.RootFS.Layers[$i])" -ForegroundColor Green
            }
            else {
                Write-Host "Non-equivalent base layer - Tag $($fromInfo.RootFS.Layers[$i]) FROM $($fromInfo.RootFS.Layers[$i])" -ForegroundColor Red
            }
        }

        Write-Host "`n"
    }
}

$tagInfo = RetrieveTagInfo

if (!$UseLocalImages) {
    PullTags -TagInfo $tagInfo
}

VerifyTagEquivalence -TagInfo $tagInfo
VerifyFrom -TagInfo $tagInfo
