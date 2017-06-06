headWag
20140624

a super-simple app which wags the robot's head back-and-forth as the robot is manually pushed forward and back.
each update, the average encoder distance is obtained from the bot, and if it's different from last time we
send a head command then the headPan and headTilt are updated as sin(encoder distance).

