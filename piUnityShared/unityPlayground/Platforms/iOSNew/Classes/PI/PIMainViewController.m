//
//  MainViewController.m
//  Unity-iPhone
//
//  Created by Orion Elenzil on 20140804.
//
//

#import "PIMainViewController.h"
#import "PIAppController.h"
#import <ChromeIOS/ChromeViewController.h>
#import <ChromeIOS/PIChromeConstants.h>
#import <ChromeIOS/PINotificationCenter.h>
#import "PlayIWrapper.h"


@interface PIMainViewController ()
@property (nonatomic, strong) NSMutableSet* connectedRobots;
- (void) startListening;
- (void) stopListening;

@end

@implementation PIMainViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void) dealloc
{
    [self stopListening];
}

- (void) startListening
{
    [PINotificationCenter addObserver:self selector:@selector(onNotification:) name:gTokenChromeToApp];
}

- (void) stopListening
{
    [PINotificationCenter removeObserver:self name:gTokenChromeToApp];
}

- (void)onNotification:(NSNotification*)notification
{
    assert([notification.name isEqualToString:gTokenChromeToApp]);
    
    NSDictionary* dict = notification.object;
    
    NSString* messageName = [dict objectForKey:gTokenMessageName];
    assert(messageName != nil);
    
    if (false) {
        // no-op
    }
    else if ([messageName isEqualToString:gTokenConnectionConnected]) {
        [self onConnect:[dict objectForKey:gTokenRobot]];
    }
    else if ([messageName isEqualToString:gTokenConnectionDisconnected]) {
        [self onDisconnect:[dict objectForKey:gTokenRobot]];
    }
    
    NSLog(@"got notification: %@", dict);
}


#pragma mark connections

- (void) onConnect:(WWRobot*)robot
{
    assert(robot != nil);
    [self.connectedRobots addObject:robot];
    [PlayIWrapper didConnectWithRobot:robot ];
}

- (void) onDisconnect:(WWRobot*)robot
{
    assert(robot != nil);
    [self.connectedRobots removeObject:robot];
    [PlayIWrapper didDisconnectWithRobot:robot ];
}



- (void)stretchViewToParent:(UIView *)view
{
    UIView *parentView = [view superview];
    if (parentView == nil) {
        assert(false);
        return;
    }
    view.frame = CGRectMake(0, 0, parentView.frame.size.width, parentView.frame.size.height);
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    self.connectedRobots = [NSMutableSet set];
    
    [self startListening];

    PIAppController *unityController = (PIAppController*) [[UIApplication sharedApplication] delegate];
    UIView *unityView = (UIView *)unityController.unityView;
    UIView *containingView;
    containingView = self.view;
    containingView = self.unityViewContainer;
    [containingView addSubview:unityView];
    [containingView sendSubviewToBack:unityView];
    [self stretchViewToParent:unityView];

    [unityController resume];
    
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    //[dict setObject:gTokenOpenChrome forKey:gTokenMessageName];
    [PINotificationCenter postNotification:gTokenAppToChrome withObject:dict];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

/*
#pragma mark - Navigation

// In a storyboard-based application, you will often want to do a little preparation before navigation
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    // Get the new view controller using [segue destinationViewController].
    // Pass the selected object to the new view controller.
}
*/
@end
