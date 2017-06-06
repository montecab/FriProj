//
//  PIAppController.h
//  Unity-iPhone
//
//  Created by Orion Elenzil on 20140804.
//
//

#import "UnityAppController.h"

@interface PIAppController : UnityAppController

@property (strong, nonatomic) UIWindow *window;

- (void)createViewHierarchyImpl;
- (void) pause;
- (void) resume;

@end
