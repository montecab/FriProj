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
#import <ChromeIOS/ChromeViewController.h>
#import <ChromeIOS/WWServerHTTPManager.h>
//#import "ChromeIOS/WWCSelectionToggleView.h"



@implementation PIAppController


- (void)createViewHierarchyImpl
{
    //This is just for compiling, because the storyboard in Chrome is referencing this class,
    //but if the class is not referenced in the code, it's thrown away by the compiler
    //reference the url for more info
    //http://stackoverflow.com/questions/1725881/unknown-class-myclass-in-interface-builder-file-error-at-runtime
    //[WWCSelectionToggleView class];
    
    // load the very first controller from storyboard
    UIStoryboard *storyBoard = [UIStoryboard storyboardWithName:@"MainStoryboard" bundle:nil];
    PIMainViewController *mainVC = [storyBoard instantiateInitialViewController];
    
    //set chrome window
    [ChromeViewController setChromeAsMainContainerForWindow:self.window withRootVC:mainVC];

    // set the root controller to be my desired view controller, and the root view as well.
    _rootController = self.window.rootViewController;
    _rootView = _rootController.view;
}

-(void)applicationDidEnterBackground:(UIApplication *)application
{
    [super applicationDidEnterBackground:application];
    // note that we auto-disconnect from bots in DidEnterBackground, but auto-connect in DidBecomeActive.
    // this is because WillEnterForeground is not called as part of app-launch, but DidBecomeActive is.
    // the chart here seems accurate: http://interactivelogic.net/wp/2011/09/there-and-back-again-when-ios-apps-go-to-the-background/
    
    if ([ChromeViewController chromeViewControllerUnchecked]) {
        [[ChromeViewController chromeViewControllerUnchecked].robotManagementVC.interactionVC autoDisconnectRobotsOnAppBackground];
    }
}

- (void)applicationDidBecomeActive:(UIApplication *)application
{
    [super applicationDidBecomeActive:application];
    
    // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
    // note that we auto-disconnect from bots in DidEnterBackground, but auto-connect in DidBecomeActive.
    // this is because WillEnterForeground is not called as part of app-launch, but DidBecomeActive is.
    // the chart here seems accurate: http://interactivelogic.net/wp/2011/09/there-and-back-again-when-ios-apps-go-to-the-background/
   if ([ChromeViewController chromeViewControllerUnchecked]) {
        [[ChromeViewController chromeViewController].robotManagementVC.interactionVC autoConnectRobotsOnAppStartup];
    }
}

- (NSUInteger)application:(UIApplication *)application supportedInterfaceOrientationsForWindow:(UIWindow *)window
{
    // Override unity's settings because we want landscape only for Chrome
    // Change this if the orientation changes
    return   (1 << UIInterfaceOrientationLandscapeRight) | (1 << UIInterfaceOrientationLandscapeLeft);
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