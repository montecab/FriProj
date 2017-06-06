//
//  PISimulatedRobot.h
//
//  Created by Saurabh Gupta on 2/13/14.
//  Copyright (c) 2014 Play-i. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>
#import "HALComponentTypes.h"
#import "HALRobotTypes.h"
#import "PIObject.h"

#ifdef __cplusplus
#import "HALRobot.h"
#endif

@protocol PISimulatedRobotDelegate;

@interface PISimulatedRobot : PIObject <CBPeripheralManagerDelegate>

@property(nonatomic, assign) id<PISimulatedRobotDelegate> delegate;

@property(nonatomic, strong) CBPeripheralManager *peripheral;
@property(nonatomic, strong) NSString *serviceName;
@property(nonatomic, strong) CBMutableService *robotService;
@property(nonatomic, strong) CBMutableCharacteristic *sensorCharacteristic;
@property(nonatomic, strong) CBMutableCharacteristic *commandCharacteristic;
@property(nonatomic, strong) NSData *pendingData;

+ (BOOL)isBluetoothSupported;
+ (CBUUID *) robotServiceId;
+ (NSString *) generateServiceName;
+ (HALBotDescription_t *) robotDescription;

- (id)initWithDelegate:(id<PISimulatedRobotDelegate>)delegate;

- (void)sendToSubscribers:(NSData *)data;
- (void)sendToSubscribersJson:(NSString *)jsonString;

// advertising management
- (void)toggleAdvertising;
- (void)startAdvertising;
- (void)stopAdvertising;
- (BOOL)isAdvertising;

@end

// Simplified protocol to respond to subscribers.
@protocol PISimulatedRobotDelegate <NSObject>

// Called when the peripheral receives a new subscriber.
- (void)simRobot:(PISimulatedRobot *)simRobot centralDidSubscribe:(CBCentral *)central;
- (void)simRobot:(PISimulatedRobot *)simRobot centralDidUnsubscribe:(CBCentral *)central;
- (void)simRobot:(PISimulatedRobot *)simRobot updateState:(int)state;
- (void)simRobot:(PISimulatedRobot *)simRobot receivedBotMessage:(NSString *)jsonString;

@end