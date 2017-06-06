//
//  PIRobotManager.h
//  APIObjectiveC
//
//  Created by Kevin Liang on 3/31/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "WWError.h"
#import "PIObject.h"
#import "PIRobot.h"

#if TARGET_OS_IPHONE
#import <CoreBluetooth/CoreBluetooth.h>
#elif TARGET_OS_MAC
#import <IOBluetooth/IOBluetooth.h>
#endif


typedef enum {
    PIRobotManagerStateUnknown = 0,
    PIRobotManagerStateResetting,
    PIRobotManagerStateUnsupported,
    PIRobotManagerStateUnauthorized,
    PIRobotManagerStatePoweredOff,
    PIRobotManagerStatePoweredOn
} PIRobotManagerState;

@protocol PIRobotManagerDelegate;

@interface PIRobotManager : PIObject <CBCentralManagerDelegate, CBPeripheralDelegate, NSURLConnectionDelegate>

@property (nonatomic, readonly) PIRobotManagerState state;
@property (nonatomic, weak) id<PIRobotManagerDelegate> delegate;
@property (nonatomic) float scanPeriod;             // set by client.
@property (nonatomic, readonly) bool scanning;      // synthetic getter.

+ (PIRobotManager *)manager; // singleton, only 1 manager per app

#pragma mark - state of all robots (mutually exclusive except allKnown)
- (NSArray *) allDiscoveredRobots;
- (NSArray *) allSelectedRobots;
- (NSArray *) allConnectedRobots;
- (NSArray *) allLostRobots;
- (NSArray *) allKnownRobots;

#pragma mark - robot discover/select/connect/disconnect (all async)
- (void) selectToConnectRobot:(PIRobot *)robot;
- (void) disconnectRobot:(PIRobot *)robot;
@end


@protocol PIRobotManagerDelegate <NSObject>

- (void) manager:(PIRobotManager *)manager didDiscoverRobot:(PIRobot *)robot;
- (void) manager:(PIRobotManager *)manager didConnectRobot:(PIRobot *)robot;

@optional
- (void) manager:(PIRobotManager *)manager didSelectRobot:(PIRobot *)robot;
- (void) manager:(PIRobotManager *)manager didLoseRobot:(PIRobot *)robot;
- (void) manager:(PIRobotManager *)manager didDisconnectRobot:(PIRobot *)robot;
- (void) manager:(PIRobotManager *)manager didFailToConnectRobot:(PIRobot *)robot error:(WWError *)error;

@end
