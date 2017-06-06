//
//  mFlurryWrapper.m
//
//
//  Created by PRADA Hsiung on 13/3/8.
//
//

#import "mFlurryWrapper.h"
#import "Flurry.h"

@implementation mFlurryWrapper

extern "C" {
    const void mStartSession(const char *apiKey){
        NSString *key=[NSString stringWithFormat:@"%s",apiKey];
        NSLog(key);
        [Flurry startSession:key];
    }
    
    
    const void mStopSession(){
        //    [Flurry stopSession];
    }
    
    const void mLogEvent(const char *msg){
        NSString *m=[NSString stringWithFormat:@"%s",msg];
        NSLog(m);
        [Flurry logEvent:m ];
    }
    
    void mlogEventWithParametersTimed(const char* eventId,const char *parameters, bool timed)
    {
        NSString *params = [NSString stringWithFormat:@"%s",parameters];
        NSArray *arr = [params componentsSeparatedByString: @"\n"];
        NSMutableDictionary *pdict = [[NSMutableDictionary alloc] init] ;
        NSLog(@"Number of params %lu\n",[arr count]);
        for(int i=0;i < [arr count]; i++)
        {
            NSString *str1 = [arr objectAtIndex:i];
            NSRange range = [str1 rangeOfString:@"="];
            if (range.location!=NSNotFound) {
                NSString *key = [str1 substringToIndex:range.location];
                NSString *val = [str1 substringFromIndex:range.location+1];
                NSLog(@"kv %@=%@\n",key,val);
                [pdict setObject:val forKey:key];
            }
        }
        if([pdict count]>0)
        {
            [Flurry logEvent:[NSString stringWithFormat:@"%s",eventId] withParameters:pdict timed:timed];
        }
        else
            mLogEvent(eventId);
    }
}

@end
