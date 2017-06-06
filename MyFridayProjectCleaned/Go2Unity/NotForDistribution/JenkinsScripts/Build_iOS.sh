#!/bin/bash

VERSION_NUMBER="$1"
BUILD_NUMBER="$2"
SERVER_ENV="$3"

#1.Build and Archive
unityPath=$(pwd)"/Go2/Go2Unity"
archivePath=$(pwd)"/Archive.xcarchive"
#remove previous Library folder to reimport all assets
rm -rf "$unityPath/Library.previous" 
mv "$unityPath/Library" "$unityPath/Library.previous" || true
/Applications/Unity/Unity.app/Contents/MacOS/Unity "$VERSION_NUMBER" "$BUILD_NUMBER" "$SERVER_ENV" -projectPath "$unityPath" -quit -batchmode -buildTarget ios -logFile -executeMethod AutoBuilder.PerformiOSBuild
xcodebuild -workspace "$unityPath"/Platforms/iOS/Unity-iPhone.xcworkspace -scheme Unity-iPhone clean archive -archivePath "$archivePath"

#2. Update log and submit to Hockeyapp
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

/usr/local/bin/puck -submit=auto -download=true -notes_path="$file" -api_token=7d076ed6a5464f109419a5ebab6b22c0 -app_id=54d2a151f36fd7f31d7829842f09eb11 "$archivePath"
