//
//  PISoundUtil.h
//  APIObjectiveC
//
//  Created by Orion Elenzil on 20140902.
//  Copyright (c) 2014 play-i. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef enum {
    WW_SAMPLE_RATE_8000HZ = 0,
    WW_SAMPLE_RATE_16000HZ,
} WWSampleRate;

typedef enum {
    WW_BITS_PER_SAMPLE_8 = 0,
    WW_BITS_PER_SAMPLE_16,
} WWBitsPerSample;

BOOL PISoundEncode(NSData *pcmData, NSMutableData *wwaData, WWSampleRate sampleRate, WWBitsPerSample bitsPerSample);

