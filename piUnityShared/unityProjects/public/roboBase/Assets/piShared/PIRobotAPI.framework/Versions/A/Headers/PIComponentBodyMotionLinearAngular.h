//
//  PIComponentBodyMotionLinearAngular.h
//  APIObjectiveC
//
//  Created by Saurabh Gupta on 7/30/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIComponent.h"

@interface PIComponentBodyMotionLinearAngular : PIComponent

@property (nonatomic) PIVelocity linearVelocity;
@property (nonatomic) PIVelocity angularVelocity;

- (id) initWithLinear:(PIVelocity)linear angular:(PIVelocity)angular;

@end
