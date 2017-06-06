//
//  PIComponentBeacon.h
//  APIObjectiveC
//
//  Created by Saurabh Gupta on 7/30/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIComponent.h"

@interface PIComponentBeacon : PIComponent

@property (nonatomic) PIBeaconLevel beaconLevel;
@property (nonatomic) PIUInteger beaconMessage;
@property (nonatomic) PIUShort beaconPeriodMs;

- (id) initWithBeaconPeriodMs:(PIUShort)periodMs;
- (void) setBeaconLevel:(PIBeaconLevel)level;
- (void) setBeaconMessage:(PIUInteger)message;
- (void) setBeaconPeriodMs:(PIUShort)periodMs;

@end
