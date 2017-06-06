//
//  PIRobot.h
//  APIObjectiveC
//
//  Created by Kevin Liang on 3/31/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#if TARGET_OS_IPHONE
#import <CoreBluetooth/CBPeripheral.h>
#elif TARGET_OS_MAC
#error TODO: deal with mac CB peripheral
#endif

#import "PIDefinitions.h"
#import "PIEventDataSource.h"
#import "PIComponent.h"
#import "PIPersonality.h"

#define PIRobotTypeIndex @"PIRobotTypeIndex"

// All the possible types of characteristics that a robot can have. Different robots may have
// one or more of these
#define PIROBOT_CHARACTERISTICS_TOTAL_COUNT 8

#define PIROBOT_COMMAND_CHARACTERISTIC_KEY @"commandKey"
#define PIROBOT_SENSOR_CHARACTERISTIC_KEY @"sensorKey"
#define PIROBOT_SPI_TO_ROBOT_CHARACTERISTIC_KEY @"spiToRobotKey"
#define PIROBOT_SPI_FROM_ROBOT_CHARACTERISTIC_KEY @"spiFromRobotKey"
#define PIROBOT_DFU_PACKET_CHARACTERISTIC_KEY @"dfuPacketKey"
#define PIROBOT_DFU_CONTROL_CHARACTERISTIC_KEY @"dfuControlKey"
#define PIROBOT_FILE_TRANSFER_PACKET_CHARACTERISTIC_KEY @"filePacketKey"
#define PIROBOT_FILE_TRANSFER_CONTROL_CHARACTERISTIC_KEY @"fileControlKey"

// todo: do we need additional [internal ?] states such as SELECTING and DISCONNECTING ?
typedef enum
{
    ROBOT_CONNECTION_UNKNOWN = 900, // never have been seen before should be transient state
    ROBOT_CONNECTION_LOST, // seen before, but is not associated at the moment
    ROBOT_CONNECTION_DISCOVERED, // waiting to be selected
    ROBOT_CONNECTION_SELECTED, // pending further action
    ROBOT_CONNECTION_CONNECTED // ready to take action
} PIRobotConnectionState;

@class PIRobotManager, PICommandSequence, PICommand, PIRobotState, PICommandSequenceExecution, PIComponentsContainer, PIRobotStateHistory;
@protocol PIRobotDelegate;
@protocol PIRobotDFUTargetAdapterDelegate;

@interface PIRobot : PIEventDataSource <CBPeripheralDelegate>

// RobotType. This is initialized to the correct value based on advertisement package
@property (nonatomic, readonly) PIRobotType robotType;
@property (nonatomic, readonly) NSString *uuId;
@property (nonatomic, strong) NSString *name;
@property (nonatomic, strong) PIRobotStateHistory *history;
@property (nonatomic, readonly) PIRobotConnectionState connectionState;
@property (nonatomic, strong) PIPersonality *personality;
@property (nonatomic, weak) id<PIRobotDelegate> delegate;
@property (nonatomic, readonly) WWDisconnectReason lastDisconnectReason;
@property (nonatomic, readonly) NSTimeInterval secondsSinceLastDisconnect;


# pragma mark - sending commands
- (void) executeCommandSequence:(PICommandSequence *)sequence withOptions:(NSDictionary *)options;
- (void) stopCommandSequence:(PICommandSequence *)sequence;
- (NSDictionary *) commandSequenceResults:(PICommandSequence *)sequence;
- (NSArray *) allExecutingCommandSequences;
- (BOOL) isExecutingCommandSequence:(PICommandSequence *)sequence;
- (void) sendRobotCommand:(PICommand *)command;

#pragma mark - overriden by child class
- (BOOL) validCommandComponentIndex:(PIComponentId)index;

#pragma fileTransfer
- (BOOL) doesSupportFileTransfer;
- (BOOL) doesSupportFirmwareUpgrade;
- (void) rebootToFirmwareUpgrade;
- (void) setSoundFileURL:(NSURL *) URL;
- (void) setFirmwareFileURL:(NSURL *) URL;
- (void) startSoundFileTransfer;
- (void) startFirmwareUpdate;
- (BOOL) soundTransferInProgress;
@end


@protocol PIRobotDelegate <NSObject>

@optional
- (void) robot:(PIRobot *)robot didStopExecutingCommandSequence:(PICommandSequence *)sequence withResults:(NSDictionary *)results;
- (void) robot:(PIRobot *)robot didFinishCommandSequence:(PICommandSequence *)sequence;
- (void) robot:(PIRobot *)robot eventsTriggered:(NSArray *)events;
- (void) robot:(PIRobot *)robot systemEventsTriggered:(NSArray *)events;
- (void) robot:(PIRobot *)robot didReceiveRobotState:(PIRobotState *)state;

@end
