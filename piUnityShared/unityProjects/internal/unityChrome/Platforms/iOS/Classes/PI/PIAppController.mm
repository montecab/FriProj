//
//  PIAppController.mm
//  Unity-iPhone
//
//  Created by Orion Elenzil on 20140804.
//
//

#import "PIAppController.h"
#import "UnityInterface.h"  // used for pause/unpause unity.
#import "PIMainViewController.h"


@implementation PIAppController

- (void)createViewHierarchyImpl
{
    // load the very first controller from storyboard
    UIStoryboard *storyBoard = [UIStoryboard storyboardWithName:@"MainStoryboard" bundle:nil];
    PIMainViewController *mainVC = [storyBoard instantiateInitialViewController];
    
    // set the root controller to be my desired view controller, and the root view as well.
    _rootController = mainVC;
    _rootView = _rootController.view;
}

// helper functions that pause/resume unity.
// This will save performance when unity is hidden and not active in the app
- (void) pause
{
    UnityPause(YES);
}

- (void) resume
{
    UnityPause(NO);
}

@end

// need this line so Unity main will know that this class is a subclass of UnityAppController,
// and so "createViewHiearchyImpl" will be called.
IMPL_APP_CONTROLLER_SUBCLASS(PIAppController)