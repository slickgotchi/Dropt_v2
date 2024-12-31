#!/bin/bash

echo "Resolving all conflicts by accepting theirs (remote version)..."

# Ensure this is a Git repository
if [ ! -d .git ]; then
    echo "Error: This script must be run in the root of a Git repository."
    exit 1
fi

# Loop through all conflicted files
git diff --name-only --diff-filter=U | while read -r file; do
    echo "Resolving conflict for $file by accepting theirs..."
    git checkout --theirs "$file"
    git add "$file"
done

echo "All conflicts resolved by accepting theirs."

