#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

find . -name "*.json" ! -name "trMissionListInfo.json" -exec ./missionSummary.py {} \; | grep "Error:"

popd

