using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    public class ALRadialMenu : UIButton
    {
        private UITapGestureRecognizer _dismissGesture;
        private bool _dismissOnOverlayTap = true;
        private UIView _overlayView;
        private double _delay;
        private IList<ALRadialMenuButton> _buttons;
        private float _radius = 100f;
        private Angle _startAngle = new Angle { Degrees = 270f };
        private Angle _circumference = new Angle { Degrees = 360f };
        private Angle _spacingDegrees;
        private CGPoint _animationOrigin;
        private readonly NSObject _orientationToken;
        private const UIViewAnimationOptions AnimationOptions = UIViewAnimationOptions.CurveEaseInOut;

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

        public ALRadialMenu() : this(CGRect.Empty) { }

        public ALRadialMenu(CGRect frame) : base(frame)
        {
            _orientationToken = UIDevice.Notifications.ObserveOrientationDidChange(OnOrientationChanged);
            CommonInit();
        }

        private void OnOrientationChanged(object sender, NSNotificationEventArgs args)
        {
            CommonInit();
        }

        private void CommonInit()
        {
            _dismissGesture?.Dispose();
            _dismissGesture = null;

            _overlayView?.RemoveFromSuperview();
            _overlayView?.Dispose();
            _overlayView = null;

            _dismissGesture = new UITapGestureRecognizer(() => Dismiss())
            {
                Enabled = _dismissOnOverlayTap
            };

            _overlayView = new UIView(UIScreen.MainScreen.Bounds);
            _overlayView.AddGestureRecognizer(_dismissGesture);
        }

        /// <summary>
        /// Set the delay between each of the buttons appearing.
        /// </summary>
        /// <param name="delay"><see cref="double"/> with the delay in ms</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu SetDelay(double delay)
        {
            _delay = delay;
            return this;
        }

        /// <summary>
        /// Set the buttons to present.
        /// </summary>
        /// <param name="buttons"><see cref="IList{T}"/> of <see cref="ALRadialMenuButton"/>.</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
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
                    action?.Invoke();
                };
            }

            return this;
        }

        /// <summary>
        /// Set whether to dismiss the menu when tapping elsewhere in the view.
        /// 
        /// Default: <value>true</value>
        /// </summary>
        /// <param name="dismissOnOverlayTap"><see cref="bool"/></param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu SetDismissOnOverlayTap(bool dismissOnOverlayTap)
        {
            _dismissOnOverlayTap = dismissOnOverlayTap;
            _dismissGesture.Enabled = _dismissOnOverlayTap;

            return this;
        }

        /// <summary>
        /// Set the radius to present the menu in.
        /// 
        /// Default: <value>100</value>
        /// </summary>
        /// <param name="radius"><see cref="float"/> with the radius.</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu SetRadius(float radius)
        {
            _radius = radius;
            return this;
        }

        /// <summary>
        /// Set the start angle of the angle to start present the menu.
        /// 
        /// Default: <value>270</value>
        /// </summary>
        /// <param name="degrees"><see cref="float"/> with the start angle.</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu SetStartAngle(float degrees)
        {
            _startAngle = new Angle { Degrees = degrees };
            return this;
        }

        /// <summary>
        /// Set the circumference of the menu.
        /// 
        /// Default: <value>360</value>
        /// </summary>
        /// <param name="degrees"><see cref="float"/> with the circumference.</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu SetCircumference(float degrees)
        {
            Circumference = new Angle { Degrees = degrees };
            return this;
        }

        /// <summary>
        /// Set the animation origin. The point to center the menu around.
        /// </summary>
        /// <param name="animationOrigin"><see cref="CGPoint"/> with the point on the screen to center the menu around.</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu SetAnimationOrigin(CGPoint animationOrigin)
        {
            _animationOrigin = animationOrigin;
            return this;
        }

        /// <summary>
        /// Present the menu in the <see cref="UIView"/>.
        /// </summary>
        /// <param name="view"><see cref="UIView"/> to present the menu in.</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu PresentInView(UIView view) { return PresentInWindow(view.Window); }

        /// <summary>
        /// Present the menu in the <see cref="UIWindow"/>.
        /// </summary>
        /// <param name="window"><see cref="UIWindow"/> to present the menu in.</param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
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

        /// <summary>
        /// Dismiss the menu.
        /// </summary>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu Dismiss()
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
            AnimateNotify(0.5, delay, 0.7f, 0.7f, AnimationOptions, () => {
                view.Alpha = 1;
                view.Center = newCenter;
            }, null);
        }

        private void DismissAnimation(UIView view, int index)
        {
            var delay = index * _delay;

            AnimateNotify(0.5, delay, 0.7f, 0.7f, AnimationOptions,
                () => {
                    view.Alpha = 0;
                    view.Center = _animationOrigin;
                }, finished => {
                    view.RemoveFromSuperview();
                });
        }

        private void SelectedAnimation(UIView view)
        {
            AnimateNotify(0.5, 0, 0.7f, 0.7f, AnimationOptions,
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _orientationToken.Dispose();

            base.Dispose(disposing);
        }
    }
}
