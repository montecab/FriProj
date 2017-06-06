//
//  PIComponent.h
//  APIObjectiveC
//
//  Created by Kevin Liang on 3/26/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIObject.h"

// keys for lights
extern NSString *const gBrightnessKey;
extern NSString *const gLEDIndexKey;
extern NSString *const gRedKey;
extern NSString *const gGreenKey;
extern NSString *const gBlueKey;

// keys for velocity
extern NSString *const gVelocityKey;
extern NSString *const gAngleKey;

// keys for sounds and animations
extern NSString *const gSoundtrackKey;
extern NSString *const gVolumeKey;
extern NSString *const gLoopsKey;
extern NSString *const gAnimationKey;
extern NSString *const gAnimationSpeedKey;

extern NSString *const gLevelKey;
extern NSString *const gMessageKey;
extern NSString *const gPeriodKey;

extern NSString *const gLinearVelocityKey;
extern NSString *const gAngularVelocityKey;
extern NSString *const gPowerKey;

@interface PIComponent : PIObject

#pragma mark - override by child objects
- (BOOL) hasValidValues;
- (void) parseData:(NSDictionary *)data;
- (BOOL) hasSameValues:(PIComponent *)otherComponent;

@end
