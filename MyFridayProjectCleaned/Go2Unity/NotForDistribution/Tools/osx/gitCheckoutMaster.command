#!/bin/bash


DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

cd ../../../../..

cd Go2
git checkout master
cd ..

cd piUnityShared
git checkout master
cd ..

cd HardwareAbstraction
git checkout master
cd ..

cd APIObjectiveC
git checkout master
cd ..

cd ChromeIOS
git checkout master
cd ..

popd
