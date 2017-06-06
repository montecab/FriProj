#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

echo "working...."

find . -name "*.json" ! -name "trMissionListInfo.json" -exec ./missionSummary.py {} \; > /tmp/mission_summary.txt

open /tmp/mission_summary.txt



popd

