using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    [Preserve(AllMembers = true)]
    public class ALRadialMenuButton : UIButton
    {
        public ALRadialMenuButton() { }
        public ALRadialMenuButton(CGRect frame) : base(frame) { }

        private Action _action;

        /// <summary>
        /// Get or set action to perform when button is touched
        /// 
        /// Uses UIControlEvent.TouchUpInside
        /// </summary>
        public Action Action
        {
            get => _action;
            set
            {
                if (_action == null && value != null) // only add it first time
                    AddTarget(PerformAction, UIControlEvent.TouchUpInside);

                _action = value;
            }
        }

        private void PerformAction(object sender, EventArgs e) { Action?.Invoke(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Action != null)
                    RemoveTarget(PerformAction, UIControlEvent.TouchUpInside);
            }

            base.Dispose(disposing);
        }
    }
}