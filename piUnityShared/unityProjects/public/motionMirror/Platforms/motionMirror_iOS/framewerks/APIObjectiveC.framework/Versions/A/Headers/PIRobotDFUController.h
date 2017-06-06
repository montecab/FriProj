//
//  PIRobotDFUController.h
//  APIObjectiveC
//
//  Created by Saurabh Gupta on 8/24/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIDefinitions.h"
#import "DFUPacket.h"
#import "PIRobot.h"
#import "PIRobot_Internal.h"

typedef enum
{
    DFU_CONTROLLER_STATE_INIT,
    DFU_CONTROLLER_STATE_DISCOVERING,
    DFU_CONTROLLER_STATE_IDLE,
    DFU_CONTROLLER_STATE_SEND_NOTIFICATION_REQUEST,
    DFU_CONTROLLER_STATE_SEND_START_COMMAND,
    DFU_CONTROLLER_STATE_SEND_RECEIVE_COMMAND,
    DFU_CONTROLLER_STATE_SEND_FIRMWARE_DATA,
    DFU_CONTROLLER_STATE_SEND_VALIDATE_COMMAND,
    DFU_CONTROLLER_STATE_SEND_RESET,
    DFU_CONTROLLER_STATE_WAIT_RECEIPT,
    DFU_CONTROLLER_STATE_FINISHED,
    DFU_CONTROLLER_STATE_CANCELED,
} PIRobotDFUControllerState;

@protocol PIRobotDFUControllerDelegate <NSObject>
- (void) didChangeState:(PIRobotDFUControllerState) state;
- (void) didUpdateProgress:(float) progress;
- (void) didFinishTransfer;
- (void) didCancelTransfer;
- (void) didDisconnect:(NSError *) error;
@end

@interface PIRobotDFUController : NSObject <PIRobotDFUTargetAdapterDelegate>
@property id<PIRobotDFUControllerDelegate> delegate;

@property NSString *appName;
@property int appSize;

@property NSString *targetName;

- (PIRobotDFUController *) initWithDelegate:(id<PIRobotDFUControllerDelegate>) delegate;
- (NSString *) stringFromState:(PIRobotDFUControllerState) state;

- (void) setRobot:(PIRobot *)robot;
- (void) setFirmwareURL:(NSURL *) URL;

- (void) startTransfer;
- (void) pauseTransfer;
- (void) cancelTransfer;
@end
