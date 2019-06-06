[cmdletbinding()]
param(
    [string]$Repo = "https://github.com/dotnet/dotnet-docker",
    [string]$LeftBranch = "nightly",
    [string]$RightBranch = "master",
    [string]$WorkDir = "D:\RepoDiff",
    [string]$DiffTool = "C:\Program Files\Beyond Compare 4\BCompare.exe",
    [string]$DiffArgs = "/expandall"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = 'SilentlyContinue'

function Get-Branch {
    param (
        [string] $Branch
    )

    $sourceZip = "$WorkDir\$Branch.zip"
    Invoke-WebRequest -OutFile "$sourceZip" "$Repo\archive\$Branch.zip"

    $sourcePath = "$WorkDir\$Branch"
    Expand-Archive "$sourceZip" -DestinationPath "$WorkDir"

    "$WorkDir\dotnet-docker-$Branch"
}


if (!(Test-Path -PathType Container $WorkDir)) {
    New-Item -ItemType Directory -Path $WorkDir
}
else {
    Remove-Item -Recurse -Force -Path "$WorkDir\*"
}

$leftPath = Get-Branch -Branch $LeftBranch
$rightPath = Get-Branch -Branch $RightBranch

Invoke-Expression "&`"$DiffTool`" `"$leftPath`" `"$rightPath`" $DiffArgs"