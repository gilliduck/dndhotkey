param(
  [ValidateSet("current", "bump", "next", "prepare")]
  [string] $Command = "next"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ProjectFile = if ($env:PROJECT_FILE) { $env:PROJECT_FILE } else { "Directory.Build.props" }
$ChangelogFile = if ($env:CHANGELOG_FILE) { $env:CHANGELOG_FILE } else { "CHANGELOG.md" }
$TagPattern = '^\d+\.\d+\.\d+$'
$SkipPattern = '\[skip-release\]|\[skip release\]'

function Get-LatestSemverTag {
  $tags = @(git tag --list |
    Where-Object { $_ -match $TagPattern } |
    Sort-Object { [version]$_ })

  if ($tags.Count -eq 0) {
    return $null
  }

  return $tags[-1]
}

function Get-ProjectVersion {
  if (-not (Test-Path $ProjectFile)) {
    return $null
  }

  [xml]$document = Get-Content -Raw $ProjectFile
  return $document.Project.PropertyGroup.Version | Select-Object -First 1
}

function Get-CurrentVersion {
  $tag = Get-LatestSemverTag
  if ($tag) {
    return $tag
  }

  $projectVersion = Get-ProjectVersion
  if ($projectVersion) {
    return $projectVersion
  }

  return "0.0.0"
}

function Get-CommitRange {
  $tag = Get-LatestSemverTag
  if ($tag) {
    return "$tag..HEAD"
  }

  return "HEAD"
}

function Get-ReleaseSubjects {
  $range = Get-CommitRange
  git log --no-merges --format="%s" $range |
    Where-Object { $_ -notmatch $SkipPattern } |
    Where-Object { $_ -notmatch '^chore\(release\):' }
}

function Get-ReleaseMessages {
  $range = Get-CommitRange
  git log --no-merges --format="%s%n%b%n---END-COMMIT---" $range |
    Where-Object { $_ -notmatch $SkipPattern }
}

function Get-ReleaseBump {
  $subjects = @(Get-ReleaseSubjects)
  $messages = @(Get-ReleaseMessages)

  if (@($subjects | Where-Object { $_ -match '^[a-zA-Z]+(\([^)]+\))?!:' }).Count -gt 0 -or
      (($messages -join "`n") -match '(?m)^BREAKING CHANGE:')) {
    return "major"
  }

  if (@($subjects | Where-Object { $_ -match '^feat(\([^)]+\))?:' }).Count -gt 0) {
    return "minor"
  }

  if (@($subjects | Where-Object { $_ -match '^(fix|perf)(\([^)]+\))?:' }).Count -gt 0) {
    return "patch"
  }

  return "none"
}

function Get-IncrementedVersion([string] $Version, [string] $Bump) {
  $parts = $Version.Split(".")
  $major = [int]$parts[0]
  $minor = [int]$parts[1]
  $patch = [int]$parts[2]

  switch ($Bump) {
    "major" {
      return "$($major + 1).0.0"
    }
    "minor" {
      return "$major.$($minor + 1).0"
    }
    "patch" {
      return "$major.$minor.$($patch + 1)"
    }
    "none" {
      return "NO_RELEASE"
    }
    default {
      throw "Unknown bump type: $Bump"
    }
  }
}

function Get-NextVersion {
  return Get-IncrementedVersion (Get-CurrentVersion) (Get-ReleaseBump)
}

function Set-ProjectVersion([string] $Version) {
  [xml]$document = Get-Content -Raw $ProjectFile
  $propertyGroup = $document.Project.PropertyGroup | Select-Object -First 1
  foreach ($name in @("Version", "AssemblyVersion", "FileVersion", "InformationalVersion")) {
    $node = $propertyGroup.SelectSingleNode($name)
    if (-not $node) {
      $node = $document.CreateElement($name)
      [void]$propertyGroup.AppendChild($node)
    }

    $node.InnerText = $Version
  }

  $document.Save((Resolve-Path $ProjectFile))
}

function Add-Category([System.Text.StringBuilder] $Builder, [string] $Title, [string] $Pattern, [string[]] $Subjects) {
  $matches = @($Subjects | Where-Object { $_ -match $Pattern })
  if ($matches.Count -eq 0) {
    return
  }

  [void]$Builder.AppendLine("### $Title")
  [void]$Builder.AppendLine()
  foreach ($match in $matches) {
    [void]$Builder.AppendLine("- $match")
  }

  [void]$Builder.AppendLine()
}

function Update-Changelog([string] $Version) {
  $subjects = @(Get-ReleaseSubjects)
  $section = [System.Text.StringBuilder]::new()
  [void]$section.AppendLine()
  [void]$section.AppendLine("## v$Version ($(Get-Date -AsUTC -Format "yyyy-MM-dd"))")
  [void]$section.AppendLine()

  Add-Category $section "Breaking Changes" '^[a-zA-Z]+(\([^)]+\))?!:' $subjects
  Add-Category $section "Features" '^feat(\([^)]+\))?:' $subjects
  Add-Category $section "Bug Fixes" '^(fix|perf)(\([^)]+\))?:' $subjects
  Add-Category $section "Other Changes" '^(build|ci|docs|refactor|style|test|chore)(\([^)]+\))?:' $subjects

  if (-not (Test-Path $ChangelogFile)) {
    "# Changelog`n`n<!-- version list -->`n" | Set-Content $ChangelogFile
  }

  $content = Get-Content -Raw $ChangelogFile
  if ($content.Contains("<!-- version list -->")) {
    $content = $content.Replace("<!-- version list -->", "<!-- version list -->$section")
  }
  else {
    $firstNewline = $content.IndexOf("`n")
    if ($firstNewline -ge 0) {
      $content = $content.Insert($firstNewline + 1, $section.ToString())
    }
    else {
      $content = "$content`n$section"
    }
  }

  Set-Content -NoNewline $ChangelogFile $content
}

function Prepare-Release {
  $version = Get-NextVersion
  if ($version -eq "NO_RELEASE") {
    return "No release-worthy Conventional Commits found."
  }

  Set-ProjectVersion $version
  Update-Changelog $version
  return $version
}

switch ($Command) {
  "current" { Get-CurrentVersion }
  "bump" { Get-ReleaseBump }
  "next" { Get-NextVersion }
  "prepare" { Prepare-Release }
}
