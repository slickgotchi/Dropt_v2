#!/bin/bash

echo "Resolving 'both modified' *.cs conflicts by accepting theirs..."

# Ensure script is run in a Git repository
if [ ! -d .git ]; then
    echo "Error: This script must be run in the root of a Git repository."
    exit 1
fi

# Loop through all 'both modified' *.cs files
git status --porcelain | grep -E "^UU.*\.cs$" | while read -r line; do
    # Extract the file path
    file=$(echo "$line" | awk '{print $2}')
    
    echo "Resolving conflict for $file..."
    git checkout --theirs "$file" && git add "$file"
done

echo "All 'both modified' *.cs conflicts resolved by accepting theirs."

