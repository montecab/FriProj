#!/bin/bash

VERSION_NUMBER="$1"
BUILD_NUMBER="$2"
SERVER_ENV="$3"
DEBUG_BUILD="$4"

#1.Build and Archive
unityPath=$(pwd)"/Go2/Go2Unity"
#remove bin folder
rm -rf "$unityPath""/Platforms/Go2Android/Wonder/assets/bin"
#remove previous Library folder to reimport all assets
rm -rf "$unityPath/Library.previous" 
mv "$unityPath/Library" "$unityPath/Library.previous" || true
#build in Unity
/Applications/Unity/Unity.app/Contents/MacOS/Unity "$VERSION_NUMBER" "$BUILD_NUMBER" "$SERVER_ENV" -projectPath "$unityPath" -quit -batchmode -buildTarget android -logFile -executeMethod AutoBuilder.PerformAndroidBuild
#run gradlew for chrome repos
export ANDROID_HOME=/Users/Shared/Jenkins/Library/Android/sdk
export JAVA_HOME=/Applications/Android\ Studio.app/Contents/jre/jdk/Contents/Home
export ANDROID_NDK_HOME=/Users/Shared/Jenkins/Library/Android/android-ndk-r10e
export PATH=/usr/local/bin:/usr/bin:/bin:/usr/sbin:/sbin:$ANDROID_HOME/platform-tools:$ANDROID_HOME/tools:$JAVA_HOME:$ANDROID_NDK_HOME
touch $(pwd)/APIAndroid/local.properties
echo "ndk.dir=/Users/Shared/Jenkins/Library/Android/android-ndk-r10e" | tee $(pwd)/APIAndroid/local.properties
echo "sdk.dir=/Users/Shared/Jenkins/Library/Android/sdk" >> $(pwd)/APIAndroid/local.properties
"$unityPath""/NotForDistribution/Tools/osx/"buildAndroidAPIAndChrome.command
#remove videos
rm -f "$unityPath""/Platforms/Go2Android/Wonder/assets/"*.mp4
#build unsigned apk
DIR=$(pwd)
pushd "$DIR"
cd "$unityPath""/Platforms/Go2Android/"
./gradlew clean
if [ "$DEBUG_BUILD" = true ] ; then
    ./gradlew assembleDebug
else
    ./gradlew assembleRelease
fi

popd

#2.Update log and submit to Hockeyapp
file="./JenkinsLastGitCommit.log" #Check log file, create new if not exist
if [ ! -e "$file" ] ; then
    touch "$file"
fi
if [ ! -w "$file" ] ; then
    echo cannot write to $file
    exit 1
fi

line=$(head -n 1 ./JenkinsLastGitCommit.log) #Get last commit
commit=$(echo "$line" | awk '{print $1;}')
echo Last commit:"$commit"

if [ -z "$commit" ]; then #Get new commits and save to log file
    commits=$(git --git-dir ./Go2/.git log --oneline -n 5)
    printf "No history, get latest 5 commits:\n""$commits\n"
    echo "$commits" > "$file"
else
    commits=$(git --git-dir ./Go2/.git log --oneline "$commit"..HEAD)
    if [ -z "$commits" ]; then
        noCommits=$(printf "$commit\nNo commits since last build\n")
        echo "$noCommits"
        echo "$noCommits" > "$file"
    else
        printf "Has history, get new commits:\n""$commits\n"
        echo "$commits" > "$file"
    fi
fi
if [ "$DEBUG_BUILD" = true ] ; then
    /usr/local/bin/puck -submit=auto -download=true -notes_path="$file" -api_token=7d076ed6a5464f109419a5ebab6b22c0 -app_id=80c84ed6e19bf050647ec4171325d4fc "$unityPath""/Platforms/Go2Android/app/build/outputs/apk/app-debug.apk"
else
    /usr/local/bin/puck -submit=auto -download=true -notes_path="$file" -api_token=7d076ed6a5464f109419a5ebab6b22c0 -app_id=80c84ed6e19bf050647ec4171325d4fc "$unityPath""/Platforms/Go2Android/app/build/outputs/apk/app-release.apk"
fi
