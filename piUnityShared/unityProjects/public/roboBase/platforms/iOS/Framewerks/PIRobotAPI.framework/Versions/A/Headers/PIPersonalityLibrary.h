//
//  PIPersonalityLibrary.h
//  APIObjectiveC
//
//  Created by Orion Elenzil on 20140812.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import <APIObjectiveC/APIObjectiveC.h>
#import "PIPersonality.h"

typedef size_t PIPersonalityIndex;

@interface PIPersonalityLibrary : PIObject

+ (PIPersonalityLibrary *)library;

- (NSUInteger) maxIndex;
- (PIPersonality*)personalityForIndex:(PIPersonalityIndex)index;

@end


