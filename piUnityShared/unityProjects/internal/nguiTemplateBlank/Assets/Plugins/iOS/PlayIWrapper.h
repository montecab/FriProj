//
//  PlayIWrapper.h
//  Unity-iPhone
//
#import <Foundation/Foundation.h>
#import <APIObjectiveC/APIObjectiveC.h>

//extern void PIRobot_connectRobot (void);

@interface PlayIWrapper : NSObject <PIRobotManagerDelegate, PIRobotDelegate>

@end
