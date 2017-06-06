piUnityShared
=============

Play-I Unity platform shared across all unity bot-controller apps.

last updated: 20140729



Installation
------------

* Start a new Unity project.
  - this is optional.

* Clone (or download) this repo.

* Copy the "piShared" folder from inside the repo into the Assets folder of your unity project:

  Note: unfortunately git submodule (or even svn external) is not a good option here, because of Unity's .meta files.

* In your unity Assets folder, create a folder named "Plugins" (if it doesn't already exist).

* Move the folder Assets/piShared/piPlugins/iOS into Assets/Plugins.

  Optional: delete the now-empty folder Assets/piShared/piPlugins.

* Switch to Unity.

  You may get some Unity warnings about the use of symlinks with the PIRobotAPI.Framework. This is ok.

* Generic iOS app settings:
  - Hook up the default splash and icon images from piUnityShared/images.
  - Set the company name and product name meaningfully.
  - You may want to change the presentation from "Portrait" to auto-rotate w/ some orientations enabled.

* Design your unity project. See the "usage" section below.



app construction
----------------

The two core unity classes here are PIBInterface.cs, which is a wrapper around the Play-I bot interface,
and piConnectionManager.cs, which is another wrapper around PIBInterface, and helps with robot connection management.

* Connection management

  To get access to the connection manager, use the singleton accessor: piConnectionManager.Instance.

  The connection manager will detect robots as they become available - see piConnectionManager.KnownBots.

  It's up to the app itself to present discovered robots to the user and provide UI for connecting / disconnecting.

  Each robot may be connected and disconnected independently.

  It's up to the app itself to enforce connection policies: eg, only connect to a single bot at a time, etc.

  Known robots can be obtained from the piConnectionManager via the KnownBots property, or via BotsInState().

  For example, if you want a list of all robots in the CONNECTED state, call piConnectionManager.Instance.BotsInState(PI.BotConnectionState.CONNECTED).
  (The example app includes this)

* Bo and Yanna

  Currently all bots are assumed to be Bo's. This will change.

* Commands / Effectors

  To send commands to a connected robot, each robot has a set of function prefixed with "cmd_".

  These will be rolled into the robot's components in a near-term release.

  As of this writing, the following effectors are available:
  - cmd_connect(), cmd_disconnect()
  - cmd_move(), cmd_moveWithDuration()
  - cmd_headTilt(), cmd_headPan(), cmd_headMove().
  - cmd_rgbLights()
  - cmd_eyeRing()
  - cmd_playSound()
  - cmd_performJsonAnimation()


* Sensors

  To read sensors from the robot, use piRobot.<SENSOR>.

  As of this writing, the following sensors are available:
  - WheelLeft

    available on: Bo1, Bo2.

  - WheelRight

    available on: Bo1, Bo2.

    Note that both these values may be "tared", or reset to zero.

    To tare the wheels, you can either issue individual tares: WheelLeft.encoderDistance.tare(), or tare the whole robot: ConnectedBo.tare().

  - ButtonMain

    available on: Bo1, Bo2.

  - Button1

    available on: Bo1, Bo2.

  - Button2

    available on: Bo1, Bo2.

  - Button3

    available on: Bo1, Bo2.

  - Accelerometer

    available on: Yana


  - DistanceSensorFrontLeft

    available on: Bo2.

  - DistanceSensorFrontRight

    available on: Bo2.

  - DistanceSensorTail

    available on: Bo2.



* Animations

  The API can play robot animation files.

  The only complexity here is that it can take time to parse the animation file, eg 800ms for a 0.5MB file, on an iPhone 5.

  To alleviate this, the BotInterface provides an optional pre-load mechanism: you can call preloadJsonAnimation() before calling performJsonAnimation().

  Even if you don't call preloadJsonAnimation(), the first call to performJsonAnimation() will also cache the parsing for you.

  Note that preloadJsonAnimation() and unloadJsonAnimation() are methods of the BotInterface, but performJsonAnimation() is a method of a Robot.

  HowTo:
  - you need the animation file in contemporary .json format.
  - get the contents of the json file into your C# code. (eg, use a TextAsset)
  - if the file is small, you can just lazily parse it via Robot.performJsonAnimation(), and you're done.
  - if the file is large, you may want to pre-load it at some convenient time such as application start-up.
  - there's currently no facility for background-parsing of animations.

  Note:
  - If the number of different animations you play in a given execution session is unbounded (eg, downloading animations from the internet, or composing new animations),
    then you may want to make use of BotInterface.unloadJsonAnimation() as well.


To get a basic example robot-enabled app up and running:
* copy the piExample folder into your Assets folder.
* add an empty GameObject.
* attach the script piConnectionMangerExampleUI.cs to the gameobject.
* use the scene editor to attach the animation examples to the script.
* that's it !

  the example includes:
  - basic connection management
  - basic motor commands
  - basic light commands
  - several animation examples



compilation
-----------

* in Unity, Build your iOS project to create an xcode project.
  - choose "Symlink Unity libraries".
    - I recommend using "Platforms/iOS" as the output folder. (eg, export as "iOS" in folder "Platforms").
  - if the xcode project already exists, be sure to choose "Append", not "Replace".
  - if you're working with an exising Unity project and get an error along the lines of this:
    - Error building Player: FileNotFoundException: Could not find file ".../Temp/StagingArea/iPhone-Trampoline/Unity-iPhone.xcodeproj/User.pbxuser",
    then you can try closing Unity, deleting the "Temp" folder, and re-exporting.
    - Alternatively, try just copying an existing .pbxuser file in ./platforms/iOS/Unity-iPhone.xcodeproj/ to one that's named how Unity wants it.

* in your xcode project:
  - add the system framework CoreBluetooth.
  - add the system framework Security.
  - add the play-I framework Assets/piUnityShared/PIRobotAPI.framework.
    - do not choose "copy item into destination folder".
  - in Build Settings | Build Options, set "Debug Information Format" to "DWARF".
    - this is optional, but dramatically speeds up xcode build-times.

* you should be ready to go.


updating to latest
------------------

From time to time, either the PIRobotAPI framework and/or the Play-I Unity Wrapper may be updated, or there may be additions to other shared code.

The strategy for this situation is:
* grab the latest version of the piUnityShared repo.
* re-copy the folder "piShared" into your unity project's "Assets" folder.
  - overwrite exisiting files if asked.
* delete "Assets/Plugins/iOS".
* move the "piPlugins/iOS" folder into "Assets/Plugins" again.


modifying the shared files
--------------------------

The story here is a little crude.

You'll need to simply modify the files in the piUnityShared repo, test against your project, and commit.




TODO
----

* refactor robot "cmd_" commands to be part of the corresponding component.
* add observer/listener system for robot events such as button press, audio, etc.













REBUILDING APIOBJECTIVEC FOR UNITY EDITOR
-----------------------------------------

* use XCode to open the APIObjectiveC workspace.
* select the target "APIObjectiveCMacBundle". (not "APIObjectiveCMac" !)
* build
* find where XCode put the product. I'm not sure how to get this out of XCode itself, but in practise it ends up somewhere like this:
  `~/Library/Developer/Xcode/DerivedData/APIObjectiveC-bjaimhentbsorvbdemnrecodfeuz/Build/Products/Debug/APIObjectiveC.bundle`.
* QUIT UNITY
  this is critical for unity to pick up the new version of the bundle.
* Copy the bundle into the appropriate spot in piUnityShared, eg:
  `cp -r "~/Library/Developer/Xcode/DerivedData/APIObjectiveC-bjaimhentbsorvbdemnrecodfeuz/Build/Products/Debug/APIObjectiveC.bundle" ~/git/playi/piUnityShared/Plugins/x86_64/ww/.`
* Done! Relaunch Unity.











