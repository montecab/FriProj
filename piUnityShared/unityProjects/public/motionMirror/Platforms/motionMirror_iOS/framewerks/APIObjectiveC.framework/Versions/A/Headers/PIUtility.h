//
//  PIUtility.h
//  APIObjectiveC
//
//  Created by Kevin Liang on 3/31/14.
//  Copyright (c) 2014 play-i. All rights reserved.
//

// cheep way to count how many times a line of code is reached.
#ifdef __cplusplus
extern "C" {
#endif
#define PI_REPORT_RATE(LABEL, LOGGINGFUNC)                                  \
if (true) {                                                                 \
    static size_t count = 0;                                                \
    static time_t lastReportMoment = 0;                                     \
    static time_t reportPeriod = 1;                                         \
                                                                            \
    count += 1;                                                             \
                                                                            \
    time_t currentMoment = time(NULL);                                      \
    if (lastReportMoment == 0) {                                            \
        lastReportMoment = currentMoment;                                   \
    }                                                                       \
                                                                            \
    time_t dT = currentMoment - lastReportMoment;                           \
    if (dT >= reportPeriod) {                                               \
        lastReportMoment = currentMoment;                                   \
        static size_t countAtLastReport = 0;                                \
        size_t dCount = count - countAtLastReport;                          \
                                                                            \
        countAtLastReport = count;                                          \
        double rate = (double)dCount / (double)dT;                          \
        LOGGINGFUNC(@"%s: total: %8ld - %5.2f/s", LABEL, count, rate);      \
    }                                                                       \
}
#ifdef __cplusplus
}
#endif

@interface PIUtility : NSObject

+ (BOOL) isKey:(id)key equalToExpectedKey:(NSString *)expectedKey;
+ (BOOL) isValue:(id)value equalToExpectedValueType:(Class)valueClass;
+ (BOOL) isBlank:(NSString *)str;

@end
