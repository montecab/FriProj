#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR" > /dev/null

cd ../../.. > /dev/null

PLATFORM_DIR_OSX=`pwd`"/Platforms/osx/wonder"

if [ -z ${UNITY_APP+x} ]; then 
	echo "please set the environment variable UNITY_APP. typically this would be /Applications/Unity/Unity.app/Contents/MacOS/Unity"
	exit 1
fi

if [ -z ${UNITY_SERIAL+x} ]; then 
	echo "please set the environment variable UNITY_SERIAL."
	exit 1
fi

echo "building.."

export UNITY_ERROR_PATH="/tmp/unity_build_error.txt"

if ! "$UNITY_APP" -serial "$UNITY_SERIAL" -batchmode -projectPath `pwd` -quit -executeMethod wwBuild.build -platform osx "$PLATFORM_DIR_OSX" ; then
	printf "\033[0;31m"
	echo "build failed:"
	cat "$UNITY_ERROR_PATH"
	printf "\033[0m"
	echo "check for more details in unity log (\$HOME/Library/Logs/Unity/Editor.log)"
	exit 1
fi

echo "build succeeded. it's in $PLATFORM_DIR_OSX. let's move it to hockey"

popd > /dev/null
