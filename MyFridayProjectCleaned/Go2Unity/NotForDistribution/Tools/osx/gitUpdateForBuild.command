#!/bin/bash


DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

./gitCheckoutRelease.command

cd ../../../../..

cd Go2
git pull
cd ..

cd piUnityShared
git pull
cd ..

cd HardwareAbstraction
git pull
cd ..

cd APIObjectiveC
git pull
cd ..

cd ChromeIOS
git pull
cd ..

popd
