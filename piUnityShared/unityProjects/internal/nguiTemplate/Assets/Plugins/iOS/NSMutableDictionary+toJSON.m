//
//  NSMutableDictionary+toJSON.m
//  Unity-iPhone
//
//

#import "NSMutableDictionary+toJSON.h"

@implementation NSMutableDictionary (toJSON)

-(NSString *)toJSON
{
    NSData *jsonData = [NSJSONSerialization
                        dataWithJSONObject:self
                        options:NSJSONWritingPrettyPrinted
                        error:nil];
    if ([jsonData length] > 0 ){
        NSString *jsonString = [[NSString alloc] initWithData:jsonData
                                                     encoding:NSUTF8StringEncoding];
//      NSLog(@"JSON:%@", jsonString);
        return jsonString;
    }
    return nil;
}

@end
