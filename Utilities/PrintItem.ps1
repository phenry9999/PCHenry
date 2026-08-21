param(
  [Parameter(Mandatory, Position = 0)] [int]$Id
)

$ErrorActionPreference = 'Stop'

# --- Config -----------------------------------------------------------------
$Org  = "swks-DAS-TestAutomation"
$Proj = "SkySpec2"
$Pat  = $env:AzureDevOpsPrintToken

if ([string]::IsNullOrWhiteSpace($Pat)) {
    Write-Error "Environment variable 'AzureDevOpsPrintToken' is not set. Run: setx AzureDevOpsPrintToken ""<your PAT>"" and reopen PowerShell."
    exit 1
}

# --- Auth -------------------------------------------------------------------
$headers = @{
    Authorization = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$Pat"))
}

# --- Helpers ----------------------------------------------------------------
function Format-Size([long]$bytes) {
    if ($bytes -ge 1MB) { return "{0:N2} MB" -f ($bytes / 1MB) }
    if ($bytes -ge 1KB) { return "{0:N2} KB" -f ($bytes / 1KB) }
    return "$bytes B"
}

function HtmlEncode([string]$s) {
    if ($null -eq $s) { return "" }
    return [System.Net.WebUtility]::HtmlEncode($s)
}

# --- Fetch work item, comments, and history ---------------------------------
$base = "https://dev.azure.com/$Org/$Proj/_apis/wit/workitems/$Id"

try {
    $wi = Invoke-RestMethod -Headers $headers `
        -Uri "$base`?`$expand=all&api-version=7.1"
}
catch {
    if ([int]$_.Exception.Response.StatusCode -in 401, 403, 404) {
        Write-Warning "Work item $Id does not exist or you do not have permission to read it."
        exit 1
    }

    throw
}

$comments = Invoke-RestMethod -Headers $headers `
    -Uri "https://dev.azure.com/$Org/$Proj/_apis/wit/workItems/$Id/comments?api-version=7.1-preview.4"

$history = Invoke-RestMethod -Headers $headers `
    -Uri "$base/updates?api-version=7.1"

$f = $wi.fields

$createdDate = [DateTimeOffset]::Parse(
    $f.'System.CreatedDate'
).ToLocalTime().ToString("dd-MM-yyyy")

$lastEditedDate = [DateTimeOffset]::Parse(
    $f.'System.ChangedDate'
).ToLocalTime().ToString("dd-MM-yyyy")

$datePrinted = (Get-Date).ToString("dd-MM-yyyy hh:mm tt")

# --- Build high-level history events ----------------------------------------
$historyFieldNames = @{
    "System.Title"                    = "Title"
    "System.State"                    = "State"
    "System.AssignedTo"               = "Assigned To"
    "System.IterationPath"            = "Iteration Path"
    "System.AreaPath"                 = "Area Path"
    "System.Tags"                     = "Tags"
    "System.Reason"                   = "Reason"
    "Microsoft.VSTS.Common.Priority"  = "Priority"
    "Microsoft.VSTS.Common.Severity"  = "Severity"
    "Microsoft.VSTS.Common.StackRank" = "Stack Rank"
}

$historyEvents = @()

foreach ($revision in $history.value) {
    if ($revision.rev -eq 1) {
        $dateValue = $revision.fields.'System.CreatedDate'.newValue
    }
    else {
        $dateValue = $revision.fields.'System.ChangedDate'.newValue
    }

    if (-not $dateValue) {
        continue
    }

    $changedOn = [DateTimeOffset]::Parse($dateValue).ToLocalTime()
    $changedBy = $revision.revisedBy.displayName

    if ($revision.rev -eq 1) {
        $historyEvents += [pscustomobject]@{
            Date = $changedOn
            Text = "$(HtmlEncode $changedBy) created the $(HtmlEncode $f.'System.WorkItemType')"
        }

        continue
    }

    foreach ($field in $revision.fields.PSObject.Properties) {
        if (-not $historyFieldNames.ContainsKey($field.Name)) {
            continue
        }

        $fieldName = $historyFieldNames[$field.Name]
        $newValue = $field.Value.newValue

        if ($null -ne $newValue -and $newValue.PSObject.Properties["displayName"]) {
            $newValue = $newValue.displayName
        }

        if ($null -eq $newValue -or [string]::IsNullOrWhiteSpace([string]$newValue)) {
            $newValue = "(empty)"
        }

        $historyEvents += [pscustomobject]@{
            Date = $changedOn
            Text = "$(HtmlEncode $changedBy) changed $(HtmlEncode $fieldName) to $(HtmlEncode ([string]$newValue))"
        }
    }
}

foreach ($comment in $comments.comments) {
    $historyEvents += [pscustomobject]@{
        Date = [DateTimeOffset]::Parse($comment.createdDate).ToLocalTime()
        Text = "$(HtmlEncode $comment.createdBy.displayName) added a comment"
    }
}

$historyEvents = @($historyEvents | Sort-Object Date -Descending)

# --- Classify relations -----------------------------------------------------
$linkRels = @{
    "System.LinkTypes.Hierarchy-Reverse"      = "Parent"
    "System.LinkTypes.Hierarchy-Forward"      = "Child"
    "System.LinkTypes.Related"                = "Related"
    "System.LinkTypes.Dependency-Forward"     = "Successor"
    "System.LinkTypes.Dependency-Reverse"     = "Predecessor"
    "System.LinkTypes.Duplicate-Forward"      = "Duplicate Of"
    "System.LinkTypes.Duplicate-Reverse"      = "Duplicated By"
    "Microsoft.VSTS.Common.TestedBy-Forward"  = "Tested By"
    "Microsoft.VSTS.Common.TestedBy-Reverse"  = "Tests"
    "System.LinkTypes.Remote.Related"         = "Remote Related"
}

$attachments  = @()
$relatedLinks = @()

if ($wi.relations) {
    $attachments = @(
        $wi.relations |
            Where-Object { $_.rel -eq "AttachedFile" }
    )

    $relatedLinks = @(
        $wi.relations |
            Where-Object { $linkRels.ContainsKey($_.rel) }
    )
}

# --- Fetch related work item information ------------------------------------
$relatedInfo = @{}

if ($relatedLinks.Count -gt 0) {
    $ids = $relatedLinks |
        ForEach-Object { ($_.url -split '/')[-1] } |
        Where-Object { $_ -match '^\d+$' } |
        Select-Object -Unique

    if ($ids.Count -gt 0) {
        $idList = $ids -join ','
        $fieldsWanted = "System.Id,System.WorkItemType,System.Title,System.State,System.AssignedTo"
        $listUrl = "https://dev.azure.com/$Org/$Proj/_apis/wit/workitems?ids=$idList&fields=$fieldsWanted&api-version=7.1"

        try {
            $listResp = Invoke-RestMethod -Headers $headers -Uri $listUrl

            foreach ($item in $listResp.value) {
                $relatedInfo["$($item.id)"] = $item.fields
            }
        }
        catch {
            Write-Warning "Could not fetch related work item details: $($_.Exception.Message)"
        }
    }
}

# --- Build HTML -------------------------------------------------------------
$html = @"
<!doctype html>
<html>
<head>
<meta charset='utf-8'>
<title>WI $Id - $($f.'System.Title')</title>
<style>
  body {
    font-family: Segoe UI, Arial, sans-serif;
    margin: 24px;
    color: #222;
  }

  .title-row {
    display: flex;
    align-items: center;
    gap: 16px;
    border-bottom: 2px solid #0078d4;
  }

  h1 {
    flex: 1;
    padding-bottom: 4px;
    font-size: 20px;
    margin: 0;
  }

  .company-brand {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
    background: #01467d;
    color: white;
    padding: 4px 8px 6px;
    font-size: 18px;
    font-weight: bold;
  }

  .company-logo {
    width: 96px;
    height: auto;
  }

  h2 {
    color: #0078d4;
    margin-top: 24px;
    font-size: 15px;
  }

  .meta {
    display: grid;
    grid-template-columns: max-content 1fr max-content 1fr max-content 1fr;
    column-gap: 4px;
    row-gap: 4px;
    background: #dadbdb;
    padding: 6px;
    border-radius: 3px;
    margin: 3px 0 0;
  }

  .meta .row {
    display: contents;
  }

  .meta .label {
    font-weight: bold;
    text-align: right;
    white-space: nowrap;
  }
  .comment {
    padding: 6px 6px;
    margin: 8px 0;
    background: #fafbfc;
  }

  .who {
    font-size: 12px;
    color: #666;
  }

  .history-item {
    display: flex;
    justify-content: space-between;
    gap: 12px;
    padding: 3px 0;
    border-bottom: 1px solid #eee;
  }

  .history-date {
    color: #666;
    font-size: 12px;
    white-space: nowrap;
  }

  img {
    max-width: 100%;
  }

  table.grid {
    border-collapse: collapse;
    width: 100%;
    margin-top: 8px;
  }

  table.grid th,
  table.grid td {
    border: 1px solid #d0d7de;
    padding: 6px 8px;
    text-align: left;
    font-size: 13px;
    vertical-align: top;
  }

  table.grid th {
    background: #f3f6fb;
  }

  table.grid td.size {
    white-space: nowrap;
    text-align: right;
  }

  .empty {
    color: #666;
    font-style: italic;
  }

  .section-separator {
    border: 0;
    border-top: 2px solid #0078d4;
    margin: 3px 0 3px;
    opacity: 1;
  }

  .footer-separator {
    margin-top: 5px;
  }

  @media print {
    a {
      color: #000;
      text-decoration: none;
    }

    body {
      margin: 12px;
      font-size: 12px;
    }

    h1 {
      font-size: 16px;
      margin: 0;
      padding-bottom: 2px;
    }

    .company-brand {
      font-size: 15px;
    }

    .company-logo {
      width: 82px;
    }

    h2 {
      font-size: 13px;
      margin-top: 14px;
    }

  .meta {
    display: grid;
    grid-template-columns: max-content 1fr max-content 1fr max-content 1fr;
    column-gap: 8px;
    row-gap: 4px;
    background: #f3f6fb;
    padding: 6px;
    border-radius: 6px;
    margin: 3px 0 0;
  }

  .meta .row {
    display: contents;
  }

  .meta .label {
    font-weight: bold;
    text-align: right;
    white-space: nowrap;
  }

    .comment {
      padding: 4px 8px;
      margin: 6px 0;
    }

    .history-item {
      padding: 2px 0;
    }

    table.grid th,
    table.grid td {
      padding: 3px 5px;
      font-size: 11px;
    }
  }
</style>
</head>
<body>

<div class='title-row'>
  <h1>#$Id — $($f.'System.Title')</h1>
  <div class='company-brand'>
    <img class='company-logo' src='https://skyspec2.skyworksinc.com/images/SWLogo.png' alt='Skyworks' />
    <span>SkySpec2</span>
  </div>
</div>

<div class='meta'>
  <div class='row'>
    <span class='label'>Type:</span>
    <span>$($f.'System.WorkItemType')</span>
    <span class='label'>State:</span>
    <span>$($f.'System.State')</span>
    <span class='label'>Assigned To:</span>
    <span>$($f.'System.AssignedTo'.displayName)</span>
  </div>

  <div class='row'>
    <span class='label'>Iteration:</span>
    <span>$($f.'System.IterationPath')</span>
    <span class='label'>Area:</span>
    <span>$($f.'System.AreaPath')</span>
    <span class='label'>Tags:</span>
    <span>$($f.'System.Tags')</span>
  </div>

  <div class='row'>
    <span class='label'>Created:</span>
    <span>$createdDate</span>
    <span class='label'>Last Edited:</span>
    <span>$lastEditedDate</span>
    <span class='label'>Date Printed:</span>
    <span>$datePrinted</span>
  </div>
</div>

<hr class='section-separator' />

<h2>Description</h2>
$($f.'System.Description')

<h2>Acceptance Criteria</h2>
$($f.'Microsoft.VSTS.Common.AcceptanceCriteria')

<h2>Repro Steps</h2>
$($f.'Microsoft.VSTS.TCM.ReproSteps')

<h2>Discussion ($($comments.count))</h2>
"@

foreach ($c in $comments.comments) {
    $html += "<div class='comment'><div class='who'>$($c.createdBy.displayName) — $($c.createdDate)</div>$($c.text)</div>"
}

# --- History ----------------------------------------------------------------
$html += "<h2>History ($($historyEvents.Count))</h2>"

if ($historyEvents.Count -eq 0) {
    $html += "<div class='empty'>No history available.</div>"
}
else {
    foreach ($event in $historyEvents) {
        $date = $event.Date.ToString("MM-dd-yyyy")

        $html += @"
<div class='history-item'>
  <span>$($event.Text)</span>
  <span class='history-date'>$date</span>
</div>
"@
    }
}

# --- Related work items -----------------------------------------------------
$html += "<h2>Related Work Items ($($relatedLinks.Count))</h2>"

if ($relatedLinks.Count -gt 0) {
    $html += @"
<table class='grid'>
<thead>
<tr>
  <th>Link Type</th>
  <th>ID</th>
  <th>Type</th>
  <th>Title</th>
  <th>State</th>
  <th>Assigned To</th>
  <th>Comment</th>
</tr>
</thead>
<tbody>
"@

    foreach ($rel in $relatedLinks) {
        $linkType = $linkRels[$rel.rel]
        $relId = ($rel.url -split '/')[-1]
        $info = $relatedInfo["$relId"]

        if ($null -ne $info) {
            $type = HtmlEncode $info.'System.WorkItemType'
            $title = HtmlEncode $info.'System.Title'
            $state = HtmlEncode $info.'System.State'

            $assigned = if ($info.'System.AssignedTo') {
                HtmlEncode $info.'System.AssignedTo'.displayName
            }
            else {
                ""
            }
        }
        else {
            $type = ""
            $title = "(not accessible)"
            $state = ""
            $assigned = ""
        }

        $comment = HtmlEncode $rel.attributes.comment
        $webUrl = "https://dev.azure.com/$Org/$Proj/_workitems/edit/$relId"

        $html += @"
<tr>
  <td>$linkType</td>
  <td><a href='$webUrl'>#$relId</a></td>
  <td>$type</td>
  <td>$title</td>
  <td>$state</td>
  <td>$assigned</td>
  <td>$comment</td>
</tr>
"@
    }

    $html += "</tbody></table>"
}

# --- Attachments ------------------------------------------------------------
$html += "<h2>Attachments ($($attachments.Count))</h2>"

if ($attachments.Count -gt 0) {
    $html += @"
<table class='grid'>
<thead>
<tr>
  <th>#</th>
  <th>Name</th>
  <th>Size</th>
  <th>Added by</th>
  <th>Added on</th>
  <th>Comment</th>
  <th>Link</th>
</tr>
</thead>
<tbody>
"@

    $i = 0

    foreach ($a in $attachments) {
        $i++

        $attr = $a.attributes
        $name = HtmlEncode $attr.name
        $sizeBytes = [long]$attr.resourceSize
        $size = Format-Size $sizeBytes
        $author = HtmlEncode $attr.authorizedBy.displayName
        $added = $attr.authorizedDate
        $comment = HtmlEncode $attr.comment
        $url = $a.url

        $html += @"
<tr>
  <td>$i</td>
  <td>$name</td>
  <td class='size'>$size</td>
  <td>$author</td>
  <td>$added</td>
  <td>$comment</td>
  <td><a href='$url'>open</a></td>
</tr>
"@
    }

    $html += "</tbody></table>"

    $totalBytes = (
        $attachments |
            ForEach-Object { [long]$_.attributes.resourceSize } |
            Measure-Object -Sum
    ).Sum

    $html += @"
<div style='margin-top:6px;font-size:12px;color:#666;'>
  Total: $($attachments.Count) file(s), $(Format-Size $totalBytes)
</div>
"@
}

$html += "<hr class='section-separator footer-separator' /></body></html>"

# --- Save, open, then clean up ---------------------------------------------
$out = Join-Path $PSScriptRoot "WI-$Id.html"
$html | Out-File -Encoding utf8 $out

if (-not (Test-Path $out)) {
    Write-Error "Output file not written: $out"
    exit 1
}

Write-Host "Wrote $out ($((Get-Item $out).Length) bytes)"

Start-Process $out
Start-Sleep -Seconds 10
Remove-Item $out -Force

Write-Host "Deleted $out"