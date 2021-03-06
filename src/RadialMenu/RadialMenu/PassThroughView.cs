﻿using System;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    internal class PassThroughView : UIView
    {
        public event EventHandler OnPointInside;
        public PassThroughView() { }
        public PassThroughView(CGRect frame) : base(frame) { }

        public bool PassThroughTouchEvents { get; set; }

        public override bool PointInside(CGPoint point, UIEvent uievent)
        {
            OnPointInside?.Invoke(this, EventArgs.Empty);

            if (PassThroughTouchEvents)
                return Subviews.Any(s => !s.Hidden && s.PointInside(point, uievent));

            return base.PointInside(point, uievent);
        }
    }
}
