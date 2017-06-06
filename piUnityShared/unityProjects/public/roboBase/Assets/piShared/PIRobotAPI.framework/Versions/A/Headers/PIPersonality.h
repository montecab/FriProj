//
//  PIPersonality.h
//  APIObjectiveC
//
//  Created by Orion Elenzil on 20140812.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import "PIObject.h"
#import "PIDefinitions.h"


// macro trickery to only have to modify this one file for tokens.
// the trick is that "DO_DEFINITION" is defined only in exactly one C file.
#ifdef PI_CONSTANT_DO_DEFINITION
#define PI_CONSTANT_DECLARE_AND_DEFINE(DECLARATION, VALUE)\
DECLARATION;\
DECLARATION = VALUE;
#else
#define PI_CONSTANT_DECLARE_AND_DEFINE(DECLARATION, VALUE)\
extern DECLARATION;
#endif

PI_CONSTANT_DECLARE_AND_DEFINE(NSString* gTokenPersonalities  , @"personalities");
PI_CONSTANT_DECLARE_AND_DEFINE(NSString* gTokenPersonalityName, @"name");
PI_CONSTANT_DECLARE_AND_DEFINE(NSString* gTokenColor          , @"color");
PI_CONSTANT_DECLARE_AND_DEFINE(NSString* gTokenRed            , @"r");
PI_CONSTANT_DECLARE_AND_DEFINE(NSString* gTokenGreen          , @"g");
PI_CONSTANT_DECLARE_AND_DEFINE(NSString* gTokenBlue           , @"b");

#undef PI_CONSTANT_DECLARE_AND_DEFINE


@interface PIPersonality : PIObject

@property (nonatomic, strong) NSString *name;   // name of the personality.
@property (nonatomic) PIColor color;

@end
