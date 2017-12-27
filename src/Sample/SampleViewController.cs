using System;
using System.Collections.Generic;
using CoreGraphics;
using DK.Ostebaronen.Touch.RadialMenu;
using UIKit;

namespace Sample
{
    public class SampleViewController : UIViewController
    {
        private readonly List<ALRadialMenuButton> _buttons = new List<ALRadialMenuButton>();
        private ALRadialMenu _radialMenu;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Touch me!";

            var gesture = new UITapGestureRecognizer(ShowMenu)
            {
                CancelsTouchesInView = false
            };
            View.AddGestureRecognizer(gesture);
            View.BackgroundColor = UIColor.White;

            var colors = new[] {
                UIColor.Blue, UIColor.Brown, UIColor.Cyan, UIColor.Green, UIColor.Magenta, UIColor.Orange,
                UIColor.Purple, UIColor.Yellow
            };

            foreach(var color in colors)
                _buttons.Add(CreateRadialButton(color, () => {
                    View.BackgroundColor = color;
                }));

            _radialMenu = new ALRadialMenu().SetButtons(_buttons).SetDelay(0.125);


            // Play with the following code if you want to pass through touch events through overlay
            //.SetOverlayCancelsTouchesInView(false);

            //_someButton = new UIButton();
            //_someButton.SetTitle("Wow!", UIControlState.Normal);
            //_someButton.TouchUpInside += SomeButtonOnTouchUpInside;
            //_someButton.SizeToFit();
            //_someButton.Frame = new CGRect(GetRandomPointWithinBounds(View.Bounds), _someButton.Frame.Size);

            //View.Add(_someButton);
        }

        //private UIButton _someButton;
        //private void SomeButtonOnTouchUpInside(object sender, EventArgs eventArgs)
        //{
        //    var button = (UIButton) sender;
        //    Title = "Wow!";
        //    var newCenter = GetRandomPointWithinBounds(View.Bounds);
        //    button.Center = newCenter;
        //}

        //private static readonly Random Random = new Random();
        //private static CGPoint GetRandomPointWithinBounds(CGRect bounds)
        //{
        //    var x = Random.Next((int)bounds.Left, (int)bounds.Right);
        //    var y = Random.Next((int)bounds.Top, (int)bounds.Bottom);

        //    return new CGPoint(x, y);
        //}

        private void ShowMenu(UITapGestureRecognizer sender)
        {
            _radialMenu.SetAnimationOrigin(sender.LocationInView(View)).PresentInView(View);
        }

        private static ALRadialMenuButton CreateRadialButton(UIColor color, Action action)
        {
            var button = new ALRadialMenuButton(new CGRect(0, 0, 40, 40))
            {
                Action = action,
                ClipsToBounds = true
            };
            button.Layer.CornerRadius = 20f;
            button.Layer.BackgroundColor = color.CGColor;

            return button;
        }
    }
}
