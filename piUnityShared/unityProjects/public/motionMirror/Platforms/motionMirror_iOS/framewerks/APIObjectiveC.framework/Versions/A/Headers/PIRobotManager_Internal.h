//
//  PIRobotManager.h
//  APIObjectiveC
//
//  Created by Kevin Liang on 3/31/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIRobotManager.h"
#import "PIRobot_Internal.h"

@class PISecureStorage;

@interface PIRobotManager ()

// robot states
@property (nonatomic, strong) NSMutableArray *robots;

@property (nonatomic, strong) CBCentralManager *bluetoothLEManager;
@property (nonatomic, strong) NSTimer *scanTimer;
@property (nonatomic, strong) NSTimer *scanPauseTimer;
@property (nonatomic) bool scanPaused;  // set by manager
@property (nonatomic, strong) PISecureStorage *secureStorage;


- (void) _scanForPIRobots;
- (PIRobot *) _knownRobotFromPeripheral:(CBPeripheral *)peripheral;
- (NSArray *) robotsWithConnectionState:(PIRobotConnectionState)state;

- (void) _failedToCompleteConnectionForPeripheral:(CBPeripheral *)peripheral error:(NSError *)error;
- (void) _didCompleteConnectionForPeripheral:(CBPeripheral *)peripheral;

- (void) _robotDidAcceptConnection:(PIRobot*)robot;


// developer token authentication
- (BOOL) isAuthorizedToConnectToRobots;
- (void) fetchAuthTokenFromServer;
- (void) authenticateDeveloper;

@end
