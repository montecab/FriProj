#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

source ./util_common.sh

if ./newSum.py --fromstringscsv "$STRINGS_CSV" ; then

# open "$STRINGS_DIR"

fi

popd

echo "done. press any key to continue."
read -n 1
