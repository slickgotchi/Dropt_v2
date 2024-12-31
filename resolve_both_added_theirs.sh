#!/bin/bash

echo "Resolving 'both added' *.prefab conflicts by accepting theirs..."

# Ensure script is run in a Git repository
if [ ! -d .git ]; then
    echo "Error: This script must be run in the root of a Git repository."
    exit 1
fi

# Loop through all 'both added' *.prefab files
git status --porcelain | grep -E "^AA.*\.prefab$" | while read -r line; do
    # Extract the file path
    file=$(echo "$line" | awk '{print $2}')
    
    echo "Resolving conflict for $file..."
    git checkout --theirs "$file" && git add "$file"
done

echo "All 'both added' *.prefab conflicts resolved by accepting theirs."

