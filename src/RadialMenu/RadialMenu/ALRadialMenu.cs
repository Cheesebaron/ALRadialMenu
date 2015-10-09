using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    public class ALRadialMenu : UIButton
    {
        private UITapGestureRecognizer _dismissGesture;
        private bool _dismissOnOverlayTap = true;
        private readonly UIView _overlayView = new UIView(UIScreen.MainScreen.Bounds);
        private double _delay;
        private IList<ALRadialMenuButton> _buttons;
        private float _radius = 100f;
        private Angle _startAngle = new Angle { Degrees = 270f };
        private Angle _circumference = new Angle { Degrees = 360f };
        private Angle _spacingDegrees;
        private CGPoint _animationOrigin;
        private UIViewAnimationOptions _animationOptions =
            UIViewAnimationOptions.CurveEaseInOut | UIViewAnimationOptions.BeginFromCurrentState;

        private IList<ALRadialMenuButton> Buttons
        {
            get { return _buttons; }
            set
            {
                _buttons = value;
                CalculateSpacing();
            }
        }

        private Angle Circumference
        {
            get { return _circumference; }
            set
            {
                _circumference = value;
                CalculateSpacing();
            }
        }

        public ALRadialMenu()
            : base(CGRect.Empty)
        { CommonInit(); }

        public ALRadialMenu(CGRect frame)
            : base(frame)
        { CommonInit(); }

        private void CommonInit()
        {
            _dismissGesture = new UITapGestureRecognizer(() => Dismiss())
            {
                Enabled = _dismissOnOverlayTap
            };

            _overlayView.AddGestureRecognizer(_dismissGesture);
        }

        public ALRadialMenu SetDelay(double delay)
        {
            _delay = delay;
            return this;
        }

        public ALRadialMenu SetButtons(IList<ALRadialMenuButton> buttons)
        {
            Buttons = buttons;

            for (var i = 0; i < Buttons.Count; i++)
            {
                var button = _buttons[i];
                button.Center = Center;

                var action = button.Action;
                var index = i;
                button.Action = () => {
                    Dismiss(index);
                    action();
                };
            }

            return this;
        }

        public ALRadialMenu SetDismissOnOverlayTap(bool dismissOnOverlayTap)
        {
            _dismissOnOverlayTap = dismissOnOverlayTap;
            _dismissGesture.Enabled = _dismissOnOverlayTap;

            return this;
        }

        public ALRadialMenu SetRadius(float radius)
        {
            _radius = radius;
            return this;
        }

        public ALRadialMenu SetStartAngle(float degrees)
        {
            _startAngle = new Angle { Degrees = degrees };
            return this;
        }

        public ALRadialMenu SetCircumference(float degrees)
        {
            Circumference = new Angle { Degrees = degrees };
            return this;
        }

        public ALRadialMenu SetAnimationOrigin(CGPoint animationOrigin)
        {
            _animationOrigin = animationOrigin;
            return this;
        }

        public ALRadialMenu PresentInView(UIView view) { return PresentInWindow(view.Window); }

        public ALRadialMenu PresentInWindow(UIWindow window)
        {
            if (Buttons == null)
            {
                Debug.WriteLine("ALRadialMenu has no buttons to present");
                return this;
            }

            if (_animationOrigin.IsEmpty)
                _animationOrigin = Center;

            window.AddSubview(_overlayView);

            for (var i = 0; i < Buttons.Count; i++)
            {
                var button = _buttons[i];

                window.AddSubview(button);
                PresentAnimation(button, i);
            }

            return this;
        }

        private ALRadialMenu Dismiss()
        {
            if (Buttons == null || Buttons.Count == 0)
            {
                Debug.WriteLine("ALRadialMenu has no buttons to present");
                return this;
            }

            Dismiss(-1);

            return this;
        }

        private void Dismiss(int selectedIndex)
        {
            _overlayView.RemoveFromSuperview();

            for (var i = 0; i < Buttons.Count; i++)
            {
                if (i == selectedIndex)
                    SelectedAnimation(Buttons[i]);
                else
                    DismissAnimation(Buttons[i], i);
            }
        }

        private void PresentAnimation(UIView view, int index)
        {
            var degrees = _startAngle.Degrees + _spacingDegrees.Degrees * index;
            var newCenter = PointOnCircumference(_animationOrigin, _radius, new Angle { Degrees = degrees });
            var delay = index * _delay;

            view.Center = _animationOrigin;
            view.Alpha = 0;
            AnimateNotify(0.5, delay, 0.7f, 0.7f, _animationOptions, () => {
                view.Alpha = 1;
                view.Center = newCenter;
            }, null);
        }

        private void DismissAnimation(UIView view, int index)
        {
            var delay = index * _delay;

            AnimateNotify(0.5, delay, 0.7f, 0.7f, _animationOptions,
                () => {
                    view.Alpha = 0;
                    view.Center = _animationOrigin;
                }, finished => {
                    if (finished)
                        view.RemoveFromSuperview();
                });
        }

        private void SelectedAnimation(UIView view)
        {
            AnimateNotify(0.5, 0, 0.7f, 0.7f, _animationOptions,
                () => {
                    view.Alpha = 0;
                    view.Transform = CGAffineTransform.MakeScale(1.5f, 1.5f);
                }, finished => {
                    view.Transform = CGAffineTransform.MakeIdentity();
                    view.RemoveFromSuperview();
                });
        }

        private static CGPoint PointOnCircumference(CGPoint origin, float radius, Angle angle)
        {
            var radians = angle.ToRadians();
            var x = origin.X + radius * Math.Cos(radians);
            var y = origin.Y + radius * Math.Sin(radians);

            return new CGPoint(x, y);
        }

        private void CalculateSpacing()
        {
            if (Buttons == null || !Buttons.Any()) return;

            var count = Buttons.Count;
            if (Circumference.Degrees < 360)
                count--;

            _spacingDegrees = new Angle { Degrees = Circumference.Degrees / count };
        }
    }
}
