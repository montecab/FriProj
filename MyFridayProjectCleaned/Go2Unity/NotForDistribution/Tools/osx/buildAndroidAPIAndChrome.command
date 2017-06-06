#!/bin/bash


DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

cd ../../../../..

cd APIAndroid
./gradlew :W2AndroidAPI:install
cd ..

cd WWSharedServicesAndroid
./gradlew :ww-shared-service:install
cd ..

cd ChromeAndroid
./gradlew :ww-chrome-library:install
cd ..

popd
