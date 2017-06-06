# Wonder App for iOS/Android

How-To's: https://drive.google.com/drive/folders/0B4_Q8ueksPWzfmtEeW0tSWtZWmxxSGNydU1VcGZ6dTRINkgxLWVwS1VqLTlJYzhBcGMwSlk


-----------------------------

notes from before 20151007:

Turing has two distinct components:
* a state-machine editor
  * each state has a single behavior
  * states transition to other states based on "triggers" such as "main button" or "1 second has elapsed".
  * behaviors are currently pretty simple: set linear velocity, play an animation, set ear color, etc.
* a "behavior maker":
  * this is in the process of being prototyped, the prototype won't be ready until next week.
  * the behavior maker will map robot sensors _continuously_ to robot actuators.
      these mappings are very structured: One Actuator uses one Sensor as an input, and can invert the sensor, and map ranges of the sensor to ranges of the actuator value.
  * don't worry too much about this.

The assignment here is to just use the current Turing prototype to get familiar with the state-machine. Ignore the behavior-maker for now.

To get going with Turing prototype, you'll need the following repos on the following branches:

| repo | branch | note |
| ---- | ------ | ---- |
| HardwareAbstraction | bundle |   |
| APIObjectiveC | bundle |   |
| ChromeIOS | bundle |   |
| piUnityShared | bundle |   |
| Go2 | bundle |  <-- this is the app  |

* using unity 5.0.1, open the Go2 project and open the scene Main.unity.
* run the program in the Unity IDE
* tap "Programs"
* tap "Create"
* tap outside the dialog
   --> now you have a blank state-machine.
* tap the "Light" tab - these are different light behaviors.
* drag a light icon out onto the field - this creates _State_ 1.
* drag another light icon out onto the field - this creates State 2.
* on the field, drag State1 onto State2: this will create a state-machine _transition_ from S1 to S2.
* in the dialog that pops up, double-tap "1S". this is the _trigger_ for this transition.
* on the field, drag State2 onto State1, and choose "1S" again as the trigger.
* connect to a robot:
  ** in the upper-left, tap on "robots".
  ** your robot might already be listed. If not, tap "Start Scan".
  ** tap on your robot to connect.  (you might also consider checking the "Auto" box, so that your robot will connect automatically on app-launch)
  ** stop the scan
  ** close the "robots" list.
* Tap Run!
   ---> the state machine should execute, transitioning from state1 to state2 and back again, once per second.

ok, those are the basics.
please take some time to use the state-machine maker.
try the different triggers, etc.

a few details:

* you can choose one state as the "omni" state. this state is always active, in that any transition out of it can be triggered at any time.  so for example if you have some big state-machine with 20 states, but you want to return to state1 from any state whenever the top-button is pressed, you can just have the omni state transition to state 1 on button.
* in edit mode (not run mode), you can tap a state to see its outgoing transitions. tap it again to see its incoming transitions.
* the forward/backward/left/right actions independently set linear velocity and angular velocity. 
* the 'clap' trigger does not work when the robot motors are on.

Okay!

A few state-machines to try building:
* Ping-Pong
  ** robot drives forward until it sees an object in front
  ** then it drives backward until it sees an object behind
  ** repeat
* Follow Wall
* Turner
  ** the robot sits still until i put my foot in front of it.
  ** then it turns until it sees my foot again and then stops.
  ** if I take my foot away and put it back again, it turns in the _other_ direction until it sees my foot again, and then stops.
  ** repeat


