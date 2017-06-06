//
//  PIComponentPower.h
//  APIObjectiveC
//
//  Created by Saurabh Gupta on 8/25/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIComponent.h"

@interface PIComponentPower : PIComponent

@property (nonatomic) PIPowerState powerState;

- (id) initWithPowerState:(PIPowerState)powerState;

@end
