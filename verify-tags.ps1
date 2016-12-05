[cmdletbinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$Repo,
    [switch]$UseLocalImages,
    [Parameter(ParameterSetName='ParseReadme')]
    [string]$Readme,
    [Parameter(ParameterSetName='ParseReadme')]
    [ValidateSet("win", "linux")]
    [string]$Platform
)

# TODO - Better error handling
Set-StrictMode -Version Latest
$ErrorActionPreference="Stop"

function RetrieveTagInfo() {
# TODO - Take as input, Read from repo's readme, non-documented tags

    if ($Readme)
    {
        $tagInfo = @{}
        type $Readme |
            where { $_ -match "^\s*-\s*\[(?:``([-._:a-z0-9]+)``)(?:,\s*``([-._:a-z0-9]+)``)*\s\(\*(?<alias>.+)\*\)\]" } |
            % { $tagInfo.Add($matches['alias'], $matches) 
            Write-Host $matches['alias'] "----" $matches.Groups}
            #| Out-Null  [-._:a-z0-9]
        Write-Host $tagInfo
    }
    else {
        $tagInfo = @{
            "1.0/debian/runtime/Dockerfile" = ("1.0.1-runtime", "1.0-runtime");
            "1.0/debian/runtime-deps/Dockerfile" = ("1.0.3-runtime-deps", "1.0-runtime-deps");
            "1.0/debian/sdk/projectjson/Dockerfile" = ("1.0.3-sdk-projectjson", "1.0-sdk-projectjson");
            "1.0/debian/sdk/msbuild/Dockerfile" = ("1.0.3-sdk-msbuild", "1.0-sdk-msbuild");
            "1.1/debian/runtime/Dockerfile" = ("1.1.0-runtime", "1.1-runtime", "1-runtime", "runtime");
            "1.1/debian/runtime-deps/Dockerfile" = ("1.1.0-runtime-deps", "1.1-runtime-deps", "1-runtime-deps", "runtime-deps");
            "1.1/debian/sdk/projectjson/Dockerfile" = ("1.1.0-sdk-projectjson", "1.1-sdk-projectjson", "sdk", "latest");
            "1.1/debian/sdk/msbuild/Dockerfile" = ("1.1.0-sdk-msbuild", "1.1-sdk-msbuild");
            # "1.1.0/jessie/runtime" = ("1.1.0", "latest");
            # "1.0.1/jessie/runtime" = ("1.0.1");
        }
    }

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
            if (-NOT $?) {
                throw "Failed pulling $tag"
            }
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

        if (!$result)
        {
            Write-Host "The following tags do not reference the same image:" `
                -ForegroundColor Red
# TODO - format table
            foreach ($tag in $imageVariant.Value) {
                $info = (docker inspect "${Repo}:$tag" |
                    ConvertFrom-Json)[0]
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
