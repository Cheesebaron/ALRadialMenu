using System;
using CoreGraphics;
using UIKit;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    public class ALRadialMenuButton : UIButton
    {
        public ALRadialMenuButton() { }
        public ALRadialMenuButton(CGRect frame) : base(frame) { }

        private Action _action;
        public Action Action
        {
            get { return _action; }
            set
            {
                if (_action == null && value != null) // only add it first time
                    AddTarget(PerformAction, UIControlEvent.TouchUpInside);

                _action = value;
            }
        }

        private void PerformAction(object sender, EventArgs e) { Action?.Invoke(); }
    }
}