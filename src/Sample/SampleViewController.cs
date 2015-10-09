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
            
            var gesture = new UITapGestureRecognizer(ShowMenu);
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

            _radialMenu = new ALRadialMenu()
                .SetButtons(_buttons)
                .SetDelay(0.125);
        }

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
