#!/bin/bash

# world's most amazing bash file
# that takes a bunch of .anim files and converts them into json and .an files for in-app and on-robot use.
# uses a file "spreadsheet.txt", which is the dash & dot "more code" column from this spreadsheet:
# https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=1684983517

rm -rf intermediates
mkdir intermediates

# rename stuff
bash ./filenameFixer.sh

# make a batch file that converts everything from .ANIM to .JSON
find . -name "*.anim" | sed -e 's/\(.*\)/ruby \.\.\/\.\.\/Tools\/osx\/maya_parser.rb -m pose --input \1/' > intermediates/gen_maya2json.sh
bash intermediates/gen_maya2json.sh

# move all the .JSON files into intermediates.
find . -name "*.json" | xargs -I{} mv {} intermediates/

cat spreadsheet.txt | sed -e 's/\"\(.*\)\" \(.*\)/..\/..\/Tools\/osx\/json2robotWmood.py \"intermediates\/\1\" intermediates\/\2/' > intermediates/gen_json2robot.sh
bash intermediates/gen_json2robot.sh

# make a batch file that renames all the .JSON to .TXT
find . -name "*.json" | sed -e 's/\(^.*\)\.json/mv \1.json \1.txt/' > intermediates/gen_renameJsonTxt.sh
bash intermediates/gen_renameJsonTxt.sh

# put a bow on it
rm -rf output
mkdir output

mkdir output/Animations_InApp
echo "this folder becomes Assets/Resources/RobotResources/Animations" > output/Animations_InApp/readme.txt
mv intermediates/*.txt output/Animations_InApp

mkdir output/Animations_OnRobot
mkdir output/Animations_OnRobot/AN_Dash
mkdir output/Animations_OnRobot/AN_Dot
echo "this folder becomes Assets/Resources/RobotResources/OnRobot/Animations" > output/Animations_OnRobot/readme.txt
find ./intermediates/ -depth -name '*.AN-dash' -execdir bash -c 'mv -if -- "$1" "../output/Animations_OnRobot/AN_Dash/SYST${1/.AN-dash/AN}"' bash {} \;
find ./intermediates/ -depth -name '*.AN-dot' -execdir bash -c 'mv -if -- "$1" "../output/Animations_OnRobot/AN_Dot/SYST${1/.AN-dot/AN}"' bash {} \;
mv output/Animations_OnRobot/AN_Dash/SYSTA10046AN output/Animations_OnRobot/AN_Dash/SYSTBOOTAN
mv output/Animations_OnRobot/AN_Dash/SYSTA10047AN output/Animations_OnRobot/AN_Dash/SYSTSLEEPAN
mv output/Animations_OnRobot/AN_Dot/SYSTA10048AN output/Animations_OnRobot/AN_Dot/SYSTBOOTAN
mv output/Animations_OnRobot/AN_Dot/SYSTA10049AN output/Animations_OnRobot/AN_Dot/SYSTSLEEPAN

echo "done! your json robot files are now maybe in output/Animations_InApp, and your .an robot files are maybe in output/Animations_OnRobot."
