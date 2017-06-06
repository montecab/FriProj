#!/bin/bash



fnGitAllDefineRepos() {
	REPOS=()
	REPOS+=("APIAndroid")
	REPOS+=("APIObjectiveC")
	REPOS+=("WWSharedServicesObjC")
	REPOS+=("ChromeAndroid")
	REPOS+=("ChromeIOS")
	REPOS+=("Go2")
	REPOS+=("HardwareAbstraction")
	REPOS+=("piUnityShared")

	#and other globals
	LINE="-------------------------------------------------------------------------"
	LINE2="- - - - - - - - - - - - - - -"
	MISSING="MISSING!!"
}

fnGitAllBranches() {
	fnGitAllDefineRepos
	
	for REPO in "${REPOS[@]}"
	do
		if [ -d "$REPO" ];
		then
			pushd .  >> /dev/null
			cd "$REPO"
			BRANCH=`git rev-parse --abbrev-ref HEAD`
			printf "%s %s %s\n" "$REPO" "${LINE2:${#REPO}}" "$BRANCH"
			popd >> /dev/null
		else
			printf "%s %s %s\n" "$REPO" "${LINE2:${#REPO}}" "$MISSING"
		fi
	done
}


DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

pushd "$DIR"

# assumes PWD is Go2/a/b/c/d
cd ../../../../..


fnGitAllBranches

popd
