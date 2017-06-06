#!/bin/bash

# cd into the folder containing this bash script
DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
cd "$DIR"

cd "../../../Assets"

SRC_ROOT="."
OUTPUT_FOLDER="Resources/Strings"
OUTPUT_FILE="$OUTPUT_FOLDER/wonder_ui_dynamic_en_US.po.txt"
EXTENSION="cs"
EXTRACTION_TOOL="../../../InternalDevTools/Localization/extractCode.py"
PLACEHOLDERS="(\{+[0-9]+\}+|ROBOT|COLOR)"

#TODO i don't know how to stick the -not -path things into an array

echo "removing $OUTPUT_FILE"
rm -f "$OUTPUT_FILE"
echo "extracting.."
find -L "$SRC_ROOT" -name "*.$EXTENSION" -not -path "$SRC_ROOT/MPMP/*" -not -path "$SRC_ROOT/TextMesh Pro/*" -exec "$EXTRACTION_TOOL" --placeholders "$PLACEHOLDERS" --append --output "$OUTPUT_FILE" --input "{}" \;
echo "done"
