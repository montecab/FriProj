#!/bin/bash


DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

cd ../../../../..

cd Go2
git checkout wonder_1.0
cd ..

cd piUnityShared
git checkout wonder_1.0
cd ..

cd HardwareAbstraction
git checkout wonder_1.0
cd ..

cd APIObjectiveC
git checkout wonder_1.0
cd ..

cd ChromeIOS
git checkout wonder_1.0
cd ..

popd
