//
//  PIComponentDistanceSensor.h
//  APIObjectiveC
//
//  Created by Kevin Liang on 3/31/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIComponent.h"

@interface PIComponentDistanceSensor : PIComponent

@property (nonatomic, readonly) PIIntensity reflectance;    // the amount of emitted light we received back
@property (nonatomic, readonly) PIDistance distance;        // inferred distance. for laboratory conditions, 1 unit = 1cm.

@end
