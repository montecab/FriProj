//
//  PlayIWrapper.h
//  Unity-iPhone
//
#import <Foundation/Foundation.h>
#import <PIRobotAPI/PIRobotAPI.h>

//extern void PIRobot_connectRobot (void);

@interface PlayIWrapper : NSObject <PIRobotManagerDelegate, PIRobotDelegate>

@end
