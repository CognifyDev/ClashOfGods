#!/bin/bash
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

if [ -f "resources.txt" ]; then
    rm resources.txt
fi

touch resources.txt

SCRIPT_NAME="$(basename "${BASH_SOURCE[0]}")"

find . -type f ! -name "$SCRIPT_NAME" ! -name "resources.txt" | while read -r file; do
    relative_path="${file#./}"
    
    sha1_hash="$(sha1sum "$file" | cut -d' ' -f1)"
    
    echo "$relative_path,$sha1_hash" >> resources.txt
done
