//
//  MainViewController.m
//  Unity-iPhone
//
//  Created by Orion Elenzil on 20140804.
//
//

#import "PIMainViewController.h"
#import "PIAppController.h"

@interface PIMainViewController ()

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

    PIAppController *unityController = (PIAppController*) [[UIApplication sharedApplication] delegate];
    UIView *unityView = (UIView *)unityController.unityView;
    UIView *containingView;
    containingView = self.view;
    containingView = self.unityViewContainer;
    [containingView addSubview:unityView];
    [containingView sendSubviewToBack:unityView];
    [self stretchViewToParent:unityView];

    [unityController resume];
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
