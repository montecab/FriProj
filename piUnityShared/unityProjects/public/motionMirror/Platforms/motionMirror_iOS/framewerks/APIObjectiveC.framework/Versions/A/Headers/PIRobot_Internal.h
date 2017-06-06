//
//  PIRobot.h
//  APIObjectiveC
//
//  Created by Kevin Liang on 3/31/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIRobot.h"

// c-lib
#ifdef __cplusplus
#import "HALRobot.h"
#endif

#define ENABLE_DUPLICATE_FILTERING 0

@class PIRobotManager, PICommandSequence, PICommand, PIRobotState, PICommandSequenceExecution, PIComponentsContainer;

@interface PIRobot ()

@property (nonatomic, strong) CBPeripheral *peripheral;
@property (nonatomic, strong) NSMutableArray *events;
@property (nonatomic, strong) PIComponentsContainer *currentRobotOutputs;
@property (nonatomic, strong) NSMutableArray *executingCommandSequences;
@property (nonatomic, strong, readonly) NSMutableDictionary *supportedCharacteristicsUUIDs;
@property (nonatomic, strong, readonly) NSMutableDictionary *discoveredCharacteristics;
@property (nonatomic, readonly) int supportedCharacteristicsCount;
@property (nonatomic, readonly) int discoveredCharacteristicsCount;
@property (nonatomic, strong) PIRobotManager *manager;
@property (nonatomic) BOOL didDiscoverLastScan;
@property (nonatomic) WWDisconnectReason pendingDisconnectReason;
@property (nonatomic, readwrite) WWDisconnectReason lastDisconnectReason;
@property (nonatomic) NSTimeInterval lastDisconnectTime;            // moment when this robot last disconnected from this app instance.
@property (nonatomic) BOOL isTryingConnectWithoutPhysicalAccess;    // used by robot mgr
@property (nonatomic) BOOL shouldAllowConnectWithoutPhysicalAccess;
@property (nonatomic) BOOL dfuUpgradeOnBLBoot;

# pragma mark - bluetooth related
+ (NSArray *) _robotServiceIds;
- (id) initWithManager:(PIRobotManager *)manager peripheral:(CBPeripheral *)peripheral;
- (BOOL) _hasPeripheral:(CBPeripheral *)peripheral;

#pragma mark connection management
- (void) transitionToConnectionState:(PIRobotConnectionState)newConnectionState;
- (void) transitionToConnectionState:(PIRobotConnectionState)newConnectionState withSelectCommand:(PICommand*)command;
+ (NSString *)toStringConnectionState:(PIRobotConnectionState)state;
- (void) didDisconnectWhileSelectedOrConnected;
- (void) _willExitSelectionMode;


# pragma mark - command sequence management
- (void) _sendOutstandingSequenceCommands; // recursive step to finish outstanding sequences
- (PICommandSequenceExecution *) _executingSequenceFromSequence:(PICommandSequence *)sequence;
- (void) _removeCommandSequenceFromExecution:(PICommandSequenceExecution *)execution;

# pragma mark - sending/receiving
- (NSArray *) _serialize:(PICommand *)command;
- (void) _sendCommandPacket:(NSData *)packet;
- (void) _didReceiveRobotData:(NSData *)rawBytes;
- (PIRobotState *) _deserialize:(NSData *)rawBytes;
- (void) setAdvertisementData:(NSDictionary *)advertisementData isRepeat:(BOOL)repeat;

- (void) _didReceiveRobotSpiData:(NSData *)rawBytes;
- (void) _didReceiveRobotFileData:(NSData *)rawBytes;
- (void) _didReceiveRobotDfuData:(NSData *)rawBytes;

#pragma mark - override by child class

@end

