function GitMe-PruneLocal {
    git fetch --prune
    git branch -vv | Select-String ': gone]' | ForEach-Object { ($_ -split '\s+')[1] } | ForEach-Object { git branch -D $_ }
}
