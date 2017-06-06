#!/bin/bash

# cd into the folder containing this bash script
DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
cd "$DIR"

cd "../../../Assets"


MISSIONS_FOLDER="Resources/TuringProto/Missions"

cd "$MISSIONS_FOLDER"

OUTPUT_FOLDER="../../Strings"
OUTPUT_FILE="$OUTPUT_FOLDER/wonder_challenges_en_US.po.txt"


echo "extracting.."
rm -f "$OUTPUT_FILE"
./newSum.py --topo "$OUTPUT_FILE" --ignore_ready
echo "done"

