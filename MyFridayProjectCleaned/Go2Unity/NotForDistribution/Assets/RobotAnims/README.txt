this folder is where we process raw .anim files exported from Maya into JSON and Robot formats.

how to use it:

* Download the .anim files,
  likely from https://drive.google.com/drive/folders/0B08qXg2uOu-wdlAzTDhCWVgtaGM
  and put them in the "Behaviors" folder here.

* Edit this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=1684983517
  - use the tab "animations".
  - one row is used per animation behavior per robot, but covering all robot "moods".
  - it's okay if a given behavior does not have a .anim file for every single mood. only one is really required.  The scripts below automatically search for all the mood-specific files and ignore absentees.
  - take care when editing (or reviewing) this spreadsheet that the two columns "code" and "more code" look sane.
  the "code" column is cut-n-pasted into the C# source, and the "more code" column forms the "spreadsheet.txt" file which is a sibling to this README file.

* Copy the "code" column from the spreadsheet into trMoodyAnimations.cs in the Go2 repo.  You'll see where.

* Copy the "more code" column as the file "spreadsheet.txt", a sibling of this README file.

* Do a quick git diff of both files you just modified to make sure they reflect the changes you expect, with no surprises.

* Sometimes the animation filenames will break with the naming convention. To address this, it would have been difficult to correct the file names because the source of those files is on a hard-drive far, far away, so the errors will keep propagating back from there. Instead, there's a "filenameFixer.sh" script which just does an ad-hoc repair of all the files which need to come back to the naming convention.
So check that any new animation files match the convention, and if not, then add lines to filenameFixer.sh to fix them.

* Convert the animations from .anim to .json and .WA (wonder anim):
  run the terminal command "./doit.sh".
  this shell script does a few things worth mentioning:
  - It runs filenameFixer.sh.
  - It creates a scratch folder called "intermediates".
  - It creates an output folder called "output".
  - It first deletes both these directories.

* The doit.sh script produces two folders of output:
  - output/Animations_OnRobot
    the .WA files as they should be on the robot
  - output/Animations_InApp
    the JSON-encoded files for use in the app.
    note the file extension is ".txt", not ".json", because Unity.

* Copy the animation files from the two ouput subfolders to their appropriate places:
  - ./installIt.sh

* If animation, sound, or default spark files have changed:
  - cd <gitroot/playi>/ChromeIOS/ChromeIOSResource/firmware
  - ../firmwareTools/updateResourceFileListings.sh
  - ../firmwareTools/currentBundleVersion.sh
  - ../firmwareTools/replaceBundleVersion.sh 23 24
    Where 23 is the current resource version, and 24 is the resource version you want to change it to.

* If this is a new behavior animation that's also user-facing in Wonder, don't forget to add a new icon file for it.
  Those live in <git root/playi>/Go2/Go2Unity/Assets/Resources/Sprites/AnimationIcons .

* Do a git status / git diff to check sanity of things.
  Don't forget to consider ChromeIOS and ChromeAndroid repo's.

* check things in.
  You may have modified other repos as well as Go2 in the steps above.

Congratulations! You are now a CMS!





