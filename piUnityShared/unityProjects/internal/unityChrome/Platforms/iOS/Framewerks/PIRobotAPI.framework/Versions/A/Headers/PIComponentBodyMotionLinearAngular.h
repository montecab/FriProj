//
//  PIComponentBodyMotionLinearAngular.h
//  APIObjectiveC
//
//  Created by Saurabh Gupta on 7/30/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIComponent.h"

@interface PIComponentBodyMotionLinearAngular : PIComponent

@property (nonatomic) int linearVelocity;
@property (nonatomic) int angularVelocity;

- (id) initWithLinear:(int)linear angular:(int)angular;

@end
