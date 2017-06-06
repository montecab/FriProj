//
//  PlayIWrapper.m
//  Unity-iPhone

#import "PlayIWrapper.h"
#import "NSMutableDictionary+toJSON.h"

@interface PlayIWrapper()
// connection management
+ (void)setPIMessageReceiverName:(NSString*)name;
+ (void)sendToUnity:(NSString*)jsonMessage;
+ (void)startScan;
+ (void)connect:(NSString*)robotUUID;
+ (void)disconnect:(NSString*)robotUUID;

// robot commands
+ (void)robot:(NSString*)robotUUID setRGBLight:(PIBrightness)red green:(PIBrightness)green blue:(PIBrightness)blue forComponents:(uint32_t *)components withCount:(uint32_t)count;
+ (void)robot:(NSString*)robotUUID sendCommand:(PICommand *)command;
+ (void)robot:(NSString*)robotUUID headPan:(PIAngle)degree;
+ (void)robot:(NSString*)robotUUID headTilt:(PIAngle)degree;
+ (void)robot:(NSString*)robotUUID headPan:(PIAngle)panDegree tilt:(PIAngle)tiltDegree;
+ (void)robot:(NSString*)robotUUID moveLeftWheel:(PIVelocity)leftVelocity rightWheel:(PIVelocity)rightVelocity;
+ (void)robot:(NSString*)robotUUID moveLeftWheel:(PIVelocity)leftVelocity rightWheel:(PIVelocity)rightVelocity forDuration:(double)seconds;
+ (PICommand *)moveCommandLeftWheelVelocity:(PIVelocity)leftVelocity rightWheel:(PIVelocity)rightVelocity;
+ (void)robot:(NSString*)robotUUID eyeRing:(PIBrightness)brightness animationID:(uint16_t)animationID loops:(uint16_t)loops bitmap:(uint16_t*)bitmap bitmapCount:(uint32_t)bitmapCount;
+ (void)robot:(NSString*)robotUUID playSound:(PISoundTrackIndex)soundIndex volume:(PIUInteger)volume loopCount:(PIUInteger)loopCount;
+ (PIUInteger)loadJsonAnimation:(NSString*)jsonAnimation;
+ (void)robot:(NSString*)robotUUID performAnimation:(PIUInteger)animId;
+ (void)unloadAnimation:(PIUInteger)animId;

+ (void)robot:(NSString*)robotUUID sendRawPacket:(uint8_t*)bytes withCount:(uint32_t)count;

// helpers
+ (PIRobot*)robotForUUID:(NSString*)robotUUID;

@end

void PIInterface_setPIMessageReceiverName(char* name) {
    [PlayIWrapper setPIMessageReceiverName:[NSString stringWithUTF8String:name]];
}

void PIRobot_startScan(void) {
    [PlayIWrapper startScan];
}

void PIRobot_connect(char* robotUUID) {
    [PlayIWrapper connect:[NSString stringWithUTF8String:robotUUID]];
}

void PIRobot_disconnect(char* robotUUID) {
    [PlayIWrapper disconnect:[NSString stringWithUTF8String:robotUUID]];
}

void PIRobot_headTilt(char* robotUUID, double angle) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] headTilt:angle];
}

void PIRobot_headPan(char* robotUUID, double angle) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] headPan:angle];
}

void PIRobot_headMove(char* robotUUID, double panAngle, double tiltAngle) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] headPan:panAngle tilt:tiltAngle];
}

void PIRobot_move(char* robotUUID, double leftWheelVelocity, double rightWheelVelocity) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] moveLeftWheel:leftWheelVelocity rightWheel:rightWheelVelocity];
}

void PIRobot_moveWithDuration(char* robotUUID, double leftWheelVelocity, double rightWheelVelocity, double duration) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] moveLeftWheel:leftWheelVelocity rightWheel:rightWheelVelocity forDuration:duration];
}

void PIRobot_rgb(char* robotUUID, uint32_t red, uint32_t green, uint32_t blue, uint32_t* components, uint32_t componentCount) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] setRGBLight:red green:green blue:blue forComponents:components withCount:componentCount];
}

void PIRobot_eyeRing(char* robotUUID, uint8_t brightness, uint16_t animationID, uint16_t loops, uint16_t* bitmap, uint32_t bitmapCount) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] eyeRing:brightness animationID:animationID loops:loops bitmap:bitmap bitmapCount:bitmapCount];
}

void PIRobot_playSound(char* robotUUID, uint32_t soundIndex, uint32_t volume, uint32_t loopCount) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] playSound:(PISoundTrackIndex)soundIndex volume:(PIUInteger)volume loopCount:loopCount];
}

uint32_t PIRobot_loadJsonAnimation(char* jsonAnimation) {
    return [PlayIWrapper loadJsonAnimation:[NSString stringWithUTF8String:jsonAnimation]];
}

void PIRobot_unloadAnimation(uint32_t animId) {
    [PlayIWrapper unloadAnimation:animId];
}

void PIRobot_performAnimation(char* robotUUID, uint32_t animId) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] performAnimation:animId];
}

void PIRobot_sendRawPacket(char* robotUUID, uint8_t* bytes, uint32_t count) {
    [PlayIWrapper robot:[NSString stringWithUTF8String:robotUUID] sendRawPacket:bytes withCount:count];
}

@implementation PlayIWrapper

// TODO - this is getting ridiculous; this needs to be a regular singleton.
static PIRobotManager* manager               = nil;
static NSString*       piMessageReceiverName = nil;

static NSMutableDictionary* discoveredRobots = nil;

static uint32_t nextAnimationLoadId = 0;
static NSMutableDictionary* loadedAnimations = nil;

+ (void)setupStatics
{
    if (discoveredRobots == nil) {
        discoveredRobots = [[NSMutableDictionary alloc] init];
    }
    
    if (loadedAnimations == nil) {
        loadedAnimations = [NSMutableDictionary new];
        nextAnimationLoadId = 1;
    }
}

+ (void)setPIMessageReceiverName:(NSString*)name
{
    piMessageReceiverName = [[NSString alloc] initWithFormat:@"%@", name];
}

+ (void)sendToUnity:(NSString*)jsonMessage
{
    if (piMessageReceiverName == nil) {
        NSLog(@"error: no message receiver name is set. you must call PIInterface_setPIReceiverName(). message dropped: %@", jsonMessage);
        return;
    }
    UnitySendMessage([piMessageReceiverName cStringUsingEncoding:NSUTF8StringEncoding], "onPIRobotManagerDelegate", [jsonMessage cStringUsingEncoding:NSUTF8StringEncoding]);
}

+(void)startScan
{
    if (piMessageReceiverName == nil) {
        NSLog(@"error: no message receiver name is set. you must call PIInterface_setPIMessageReceiverName(). scan aborted.");
        return;
    }
    
    manager = [PIRobotManager manager]; // obtain singleton manager class
    // note: because we're in a Class method, the following line is passing the Class itself, not an instance.
    manager.delegate = self; // set delegate to handle callback
    
    [manager scanForPIRobots:2.0]; // periodically scan every 2 seconds.  if <= 0, then it will be a one time scan
}

+(PIRobot*)robotForUUID:(NSString*)robotUUID
{
    [self setupStatics];
    PIRobot* robot = [discoveredRobots objectForKey:robotUUID];
    if (robot == nil) {
        NSLog(@"error: attempt to access to unknown robot. uuid=%@", robotUUID);        
        [PlayIWrapper sendUnityMessageWithMethodName:@"didAttemptToAccessUnknownRobot" andUUID:robotUUID];
        return nil;
    }
    else {
        return robot;
    }
    
}

+(void)connect:(NSString *)robotUUID
{
    PIRobot* robot = [PlayIWrapper robotForUUID:robotUUID];
    if (robot != nil) {
        [manager connectRobot:robot];
    }
}

+(void)disconnect:(NSString *)robotUUID
{
    PIRobot* robot = [PlayIWrapper robotForUUID:robotUUID];
    if (robot != nil) {
        [manager disconnectRobot:robot];
    }
}

+ (void) robot:(NSString*)robotUUID setRGBLight:(PIBrightness)red green:(PIBrightness)green blue:(PIBrightness)blue forComponents:(uint32_t *)components withCount:(uint32_t)count
{
    PICommand *command = [PICommand new];
    for (unsigned int i = 0; i < count; i++) {
        PIComponentLightRGB *component = [[PIComponentLightRGB alloc] initWithRed:red green:green blue:blue];
        [command setComponent:component forIndex:components[i]];
    }
    [PlayIWrapper robot:robotUUID sendCommand:command];
}

+ (void) robot:(NSString*)robotUUID sendCommand:(PICommand *)command
{
    PIRobot* robot = [PlayIWrapper robotForUUID:robotUUID];
    if (robot != nil) {
        [robot sendRobotCommand:command];
    }
}

+ (void) robot:(NSString*)robotUUID headPan:(PIAngle)degree
{
    PICommand *command = [PICommand new];
    [command setHeadPan:[[PIComponentMotorServo alloc] initWithAngle:degree]];
    [PlayIWrapper robot:robotUUID sendCommand:command];
}

+ (void) robot:(NSString*)robotUUID headTilt:(PIAngle)degree
{
    PICommand *command = [PICommand new];
    [command setHeadTilt:[[PIComponentMotorServo alloc] initWithAngle:degree]];
    [PlayIWrapper robot:robotUUID sendCommand:command];
}

+ (void) robot:(NSString*)robotUUID headPan:(PIAngle)panDegree tilt:(PIAngle)tiltDegree
{
    PICommand *command = [PICommand new];
    PIComponentMotorServo *panMotion = [[PIComponentMotorServo alloc] initWithAngle:panDegree];
    PIComponentMotorServo *tiltMotion = [[PIComponentMotorServo alloc] initWithAngle:tiltDegree];
    [command setHeadTilt:tiltMotion pan:panMotion];
    [PlayIWrapper robot:robotUUID sendCommand:command];
}

+ (void) robot:(NSString*)robotUUID moveLeftWheel:(PIVelocity)leftVelocity rightWheel:(PIVelocity)rightVelocity
{
    PICommand *command = [self moveCommandLeftWheelVelocity:leftVelocity rightWheel:rightVelocity];
    [PlayIWrapper robot:robotUUID sendCommand:command];
}

+ (void) robot:(NSString*)robotUUID moveLeftWheel:(PIVelocity)leftVelocity rightWheel:(PIVelocity)rightVelocity forDuration:(double)seconds
{
    PICommandSequence *sequence = [PICommandSequence new];
    PICommand *moveCommand = [self moveCommandLeftWheelVelocity:leftVelocity rightWheel:rightVelocity];
    PICommand *stopCommand = [self moveCommandLeftWheelVelocity:0 rightWheel:0];
    [sequence addCommand:moveCommand withDuration:seconds];
    [sequence addCommand:stopCommand withDuration:1.0];

    PIRobot* robot = [PlayIWrapper robotForUUID:robotUUID];
    if (robot != nil) {
        [robot executeCommandSequence:sequence withOptions:nil];
    }
}

+ (PICommand *)moveCommandLeftWheelVelocity:(PIVelocity)leftVelocity rightWheel:(PIVelocity)rightVelocity
{
    PICommand *command = [PICommand new];
    PIComponentMotorWheel *leftWheel = [[PIComponentMotorWheel alloc] initWithVelocity:leftVelocity];
    PIComponentMotorWheel *rightWheel = [[PIComponentMotorWheel alloc] initWithVelocity:rightVelocity];
    [command setMotorLeft:leftWheel right:rightWheel];
    return command;
}

+ (void) robot:(NSString*)robotUUID eyeRing:(PIBrightness)brightness animationID:(uint16_t)animationID loops:(uint16_t)loops bitmap:(uint16_t*)bitmap bitmapCount:(uint32_t)bitmapCount
{
    PIComponentEyeRing *eye = [PIComponentEyeRing new];
    eye.brightness          = brightness;
    eye.animationIndex      = animationID;
    eye.loops               = loops;
    
    for (int n = 0; n < bitmapCount; ++n) {
        [eye setLEDValue:(BOOL)bitmap[n] atIndex:n];
    }
    
    PICommand *command = [PICommand new];
    [command setEyeRing:eye];
    [PlayIWrapper robot:robotUUID sendCommand:command];
}

+ (void) robot:(NSString*)robotUUID playSound:(PISoundTrackIndex)soundIndex volume:(PIUInteger)volume loopCount:(PIUInteger)loopCount
{
    PIComponentSpeaker *speaker = [[PIComponentSpeaker alloc] initWithSoundTrack:soundIndex volume:volume loops:loopCount];
    PICommand *command = [PICommand new];
    [command setComponent:speaker forIndex:COMPONENT_SPEAKER];
    [PlayIWrapper robot:robotUUID sendCommand:command];
}

+ (uint32_t)loadJsonAnimation:(NSString *)jsonAnimation
{
    [self setupStatics];
    
    NSData* jsonData = [jsonAnimation dataUsingEncoding:NSUTF8StringEncoding];
    
    PICommandSequence* sequence = [PICommandSequence sequenceFromData:jsonData];
    
    if (sequence == nil) {
        return 0;
    }
    
    uint32_t animId = nextAnimationLoadId;
    nextAnimationLoadId += 1;
    
    NSNumber* animKey = [NSNumber numberWithUnsignedInt:animId];

    [loadedAnimations setObject:sequence forKey:animKey];
    
    return animId;
}

+ (void)unloadAnimation:(uint32_t)animId
{
    [self setupStatics];

    NSNumber* animKey = [NSNumber numberWithUnsignedInt:animId];
    
    PICommandSequence* sequence = [loadedAnimations objectForKey:animKey];

    if (sequence == nil) {
        NSLog(@"unloadAnimation: unknown animation id: %@", animKey);
        return;
    }
    
    [loadedAnimations removeObjectForKey:animKey];
}

+ (void) robot:(NSString*)robotUUID performAnimation:(uint32_t)animId
{
    [self setupStatics];

    NSNumber* animKey = [NSNumber numberWithUnsignedInt:animId];
    
    PICommandSequence* sequence = [loadedAnimations objectForKey:animKey];
    
    if (sequence == nil) {
        NSLog(@"performAnimation: unknown animation id: %@", animKey);
        return;
    }
    
    PIRobot* robot = [PlayIWrapper robotForUUID:robotUUID];
    if (robot != nil) {
        [robot executeCommandSequence:sequence withOptions:nil];
    }
}

+ (void) robot:(NSString*)robotUUID sendRawPacket:(uint8_t*)bytes withCount:(uint32_t)count
{
    NSString* s = @"";
    for (size_t n = 0; n < count; ++n) {
        s = [NSString stringWithFormat: @"%@%@%2X", s, (n > 0 ? @" " : @""), bytes[n]];
    }
    
    PIRobot* robot = [PlayIWrapper robotForUUID:robotUUID];
    if (robot != nil) {
        NSLog(@"Sending raw packet: %@", s);
        
        NSData* data = [NSData dataWithBytes:bytes length:count];
        [robot _sendRawPacket:data];
    }
}


#pragma mark PIRobotManager delegate methods


+ (void) manager:(PIRobotManager *)manager didDiscoverRobot:(PIRobot *)robot
{
    [self setupStatics];
    [discoveredRobots setObject:robot forKey:robot.uuId];
    
    NSLog(@"robot connection status: %d", robot.connectionState);  // should print "robot connection status: 1"
    
    [PlayIWrapper sendUnityMessageWithMethodName:@"didDiscoverRobot" andRobot:robot];

    [manager scanForPIRobots:2.0]; // periodically scan every 2 seconds.  if <= 0, then it will be a one time scan
}

+ (void) manager:(PIRobotManager *)manager didConnectWithRobot:(PIRobot *)robot
{
    robot.delegate = self;
    
    NSLog(@"connected with %@, status: %d", robot.name, robot.connectionState);  // should print "connected with KevinBot, status: 2"
    
    [PlayIWrapper sendUnityMessageWithMethodName:@"didConnectWithRobot" andRobot:robot];

    [manager scanForPIRobots:2.0]; // periodically scan every 2 seconds.  if <= 0, then it will be a one time scan
}

+ (void) manager:(PIRobotManager *)manager didDisconnectWithRobot:(PIRobot *)robot error:(NSError *)error
{
    NSLog(@"disconnected with %@, status: %d", robot.name, robot.connectionState);

    [PlayIWrapper sendUnityMessageWithMethodName:@"didDisconnectWithRobot" andRobot:robot];
    
    [manager scanForPIRobots:2.0]; // periodically scan every 2 seconds.  if <= 0, then it will be a one time scan
}

+ (void) manager:(PIRobotManager *)manager didFailToConnectWithRobot:(PIRobot *)robot error:(NSError *)error
{
    NSLog(@"failed to connect with %@, status: %d", robot.name, robot.connectionState);
    
    [PlayIWrapper sendUnityMessageWithMethodName:@"didFailToConnectWithRobot" andRobot:robot];
    
    [manager scanForPIRobots:2.0]; // periodically scan every 2 seconds.  if <= 0, then it will be a one time scan
}

+ (void) manager:(PIRobotManager *)manager stateDidChange:(PIRobotManagerState)oldState toState:(PIRobotManagerState)newState
{
    NSLog(@"manager state changed: %ud --> %ud", oldState, newState);
}
         
#pragma mark json utilities

 + (NSDictionary*)robotToDict:(PIRobot*)robot
{
    NSMutableDictionary* ret = [NSMutableDictionary new];
    
    NSNumber* robotType  = [NSNumber numberWithInt:robot.robotType];
    
    [ret setObject:robot.name forKey:@"name"];
    [ret setObject:robot.uuId forKey:@"uuId"];
    [ret setObject:robotType  forKey:@"type"];
    
    return ret;
}
 
 +(void) sendUnityMessageWithMethodName:(NSString*)methodName andRobot:(PIRobot*)robot
{
    NSMutableDictionary* dict = [NSMutableDictionary new];
    [dict setObject:methodName                       forKey:@"method"];
    [dict setObject:[PlayIWrapper robotToDict:robot] forKey:@"robot" ];
    NSString* jsonMessage = [dict toJSON];
    [PlayIWrapper sendToUnity:jsonMessage];
}
 
 
 +(void) sendUnityMessageWithMethodName:(NSString*)methodName andUUID:(NSString*)robotUUID
{
    NSMutableDictionary* dict = [NSMutableDictionary new];
    [dict setObject:methodName forKey:@"method"];
    [dict setObject:robotUUID  forKey:@"uuId"  ];
    NSString* jsonMessage = [dict toJSON];
    [PlayIWrapper sendToUnity:jsonMessage];
}

+ (NSDictionary*)robotStateToDict:(PIRobotState*)state
{
    NSMutableDictionary* ret = [NSMutableDictionary new];
    
    NSArray* componentKeys = [state allKeys];
    
    for (id keyId in componentKeys) {
        PIComponentId key = (PIComponentId)[keyId longValue];
        
        NSMutableDictionary* componentDict = [NSMutableDictionary new];
        switch (key) {
            default:
            {
                // don't pass on.
                componentDict = nil;
                break;
            }
                
            case COMPONENT_ACCELEROMETER:
            {
                PIComponentAccelerometer* component = (PIComponentAccelerometer*)[state componentFromKey:keyId];
                [componentDict setObject:[NSNumber numberWithDouble:component.x] forKey:@"x"];
                [componentDict setObject:[NSNumber numberWithDouble:component.y] forKey:@"y"];
                [componentDict setObject:[NSNumber numberWithDouble:component.z] forKey:@"z"];
                break;
            }
                
            case COMPONENT_GYROSCOPE:
            {
                PIComponentGyroscope* component = (PIComponentGyroscope*)[state componentFromKey:keyId];
                [componentDict setObject:[NSNumber numberWithDouble:component.x] forKey:@"x"];
                [componentDict setObject:[NSNumber numberWithDouble:component.y] forKey:@"y"];
                [componentDict setObject:[NSNumber numberWithDouble:component.z] forKey:@"z"];
                break;
            }
                
            case COMPONENT_DISTANCE_SENSOR_FRONT_LEFT:
            case COMPONENT_DISTANCE_SENSOR_FRONT_RIGHT:
            {
                PIComponentDistanceSensorPair* component = (PIComponentDistanceSensorPair*)[state componentFromKey:keyId];
                [componentDict setObject:[NSNumber numberWithDouble:component.distance     ] forKey:@"distance"     ];
                [componentDict setObject:[NSNumber numberWithDouble:component.margin       ] forKey:@"margin"       ];
//              [componentDict setObject:[NSNumber numberWithDouble:component.otherDistance] forKey:@"otherDistance"];
//              [componentDict setObject:[NSNumber numberWithDouble:component.otherMargin  ] forKey:@"otherMargin"  ];
                break;
            }
                
            case COMPONENT_DISTANCE_SENSOR_TAIL:
            {
                PIComponentDistanceSensor* component = (PIComponentDistanceSensor*)[state componentFromKey:keyId];
                [componentDict setObject:[NSNumber numberWithDouble:component.distance     ] forKey:@"distance"     ];
                [componentDict setObject:[NSNumber numberWithDouble:component.margin       ] forKey:@"margin"       ];
                break;
            }
        
            case COMPONENT_MOTOR_RIGHT_WHEEL:
            case COMPONENT_MOTOR_LEFT_WHEEL:
            {
                PIComponentMotorWheel* component = (PIComponentMotorWheel*)[state componentFromKey:keyId];
                [componentDict setObject:[NSNumber numberWithDouble:component.encoderDistance] forKey:@"encoderDistance"];
                break;
            }
                
            case COMPONENT_BUTTON_MAIN:
            case COMPONENT_BUTTON_1:
            case COMPONENT_BUTTON_2:
            case COMPONENT_BUTTON_3:
            {
                PIComponentButton* component = (PIComponentButton*)[state componentFromKey:keyId];
                [componentDict setObject:[NSNumber numberWithInt:component.state] forKey:@"state"];
                break;
            }

        }
        
        if (componentDict != nil) {
            [ret setObject:componentDict forKey:[NSString stringWithFormat:@"%u", key]];
        }
    }
    
    return ret;
}

#pragma mark PIRobot delegate methods

+ (void) robot:(PIRobot *)robot didReceiveRobotState:(PIRobotState *)state
{
    NSMutableDictionary* dict = [NSMutableDictionary new];
    [dict setObject:@"didReceiveRobotState"               forKey:@"method"];
    [dict setObject:[PlayIWrapper robotToDict:robot]      forKey:@"robot" ];
    [dict setObject:[PlayIWrapper robotStateToDict:state] forKey:@"state" ];
    
    NSString* jsonMessage = [dict toJSON];
    [PlayIWrapper sendToUnity:jsonMessage];
}

+ (void) robot:(PIRobot *)robot didStopExecutingCommandSequence:(PICommandSequence *)sequence withResults:(NSDictionary *)results
{
    NSLog(@"sequence cancelled");
}

+ (void) robot:(PIRobot *)robot didFinishCommandSequence:(PICommandSequence *)sequence
{
    NSLog(@"sequence completed!");
}



@end
