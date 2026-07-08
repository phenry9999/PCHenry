function GitMe-PruneLocal {
    git fetch --prune
    git branch -vv | Select-String ': gone]' | ForEach-Object { ($_ -split '\s+')[1] } | ForEach-Object { git branch -D $_ }
}

function GitMe-DeleteBranch($branch) {
    if (-not $branch) { Write-Host "Usage: GitMe-DeleteBranch <branch-name>"; return }

    $local = git branch --list $branch
    if ($local) {
        git branch -D $branch
        Write-Host "Deleted local branch: $branch"
    } else {
        Write-Host "Skipping, local branch not found: $branch"
    }

    $remote = git ls-remote --heads origin $branch
    if ($remote) {
        git push origin --delete $branch
        Write-Host "Deleted remote branch: $branch"
    } else {
        Write-Host "Skipping, remote branch not found: $branch"
    }
}