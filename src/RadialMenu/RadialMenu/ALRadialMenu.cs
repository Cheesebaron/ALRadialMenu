using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;

namespace DK.Ostebaronen.Touch.RadialMenu
{
    [Preserve(AllMembers = true)]
    public class ALRadialMenu : UIButton
    {
        private UITapGestureRecognizer _dismissGesture;
        private bool _dismissOnOverlayTap = true;
        private bool _overlayCancelsTouchesInView = true;
        private PassThroughView _overlayView;
        private double _delay;
        private IReadOnlyList<ALRadialMenuButton> _buttons;
        private float _radius = 100f;
        private Angle _circumference = new Angle { Degrees = 360f };
        private CGPoint _animationOrigin;
        private readonly NSObject _orientationToken;
        private const UIViewAnimationOptions AnimationOptions = UIViewAnimationOptions.CurveEaseInOut;

        private IReadOnlyList<ALRadialMenuButton> Buttons
        {
            get => _buttons;
            set
            {
                _buttons = value;
                CalculateSpacing();
            }
        }

        /// <summary>
        /// Get or set the circumference of the menu
        /// </summary>
        public Angle Circumference
        {
            get => _circumference;
            set
            {
                _circumference = value;
                CalculateSpacing();
            }
        }

        /// <summary>
        /// Get the start angle
        /// </summary>
        public Angle StartAngle { get; private set; } = new Angle { Degrees = 270f };

        /// <summary>
        /// Get the spacing in degrees between buttons
        /// </summary>
        public Angle Spacing { get; private set; }

        /// <summary>
        /// Get or set the animation action to use when presenting a button
        /// </summary>
        public Action<UIView, int> PresentButtonAnimation { get; set; }

        /// <summary>
        /// Get or set the animation action to use when dismissing a button
        /// </summary>
        public Action<UIView, int> DismissButtonAnimation { get; set; }

        /// <summary>
        /// Get or set the animation action to use when a button is selected
        /// </summary>
        public Action<UIView> SelectedButtonAnimation { get; set; }

        /// <summary>
        /// This event is invoked whenever the menue is dismissed.
        /// </summary>
        public event EventHandler OnDismissing;

        /// <summary>
        /// This property is an indicator to wether the radial menu is currently unfolded or not.
        /// The property will only display false when the animation of the last button has finished and true once the animation of the first button starts.
        /// </summary>
        public bool IsUnfolded { get; private set; }

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

            _dismissGesture = new UITapGestureRecognizer(InternalDismiss)
            {
                Enabled = _dismissOnOverlayTap,
                CancelsTouchesInView = _overlayCancelsTouchesInView
            };

            _overlayView = new PassThroughView(UIScreen.MainScreen.Bounds)
            {
                PassThroughTouchEvents = !_overlayCancelsTouchesInView
            };
            _overlayView.AddGestureRecognizer(_dismissGesture);
            _overlayView.OnOverlayPointInside += OverlayView_OnOverlayPointInside;
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
            Buttons = new ReadOnlyCollection<ALRadialMenuButton>(buttons);

            for (var i = 0; i < Buttons.Count; i++)
            {
                var button = _buttons[i];
                button.Center = Center;

                var action = button.Action;
                var index = i;
                button.Action = () =>
                {
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
        /// Set whether to cancel touches on the underlying view
        /// 
        /// Default: <value>true</value>
        /// </summary>
        /// <param name="cancelsTouchesInView"><see cref="bool"/></param>
        /// <returns><see cref="ALRadialMenu"/> for method chaining.</returns>
        public ALRadialMenu SetOverlayCancelsTouchesInView(bool cancelsTouchesInView)
        {
            _overlayCancelsTouchesInView = cancelsTouchesInView;
            _dismissGesture.CancelsTouchesInView = _overlayCancelsTouchesInView;
            _overlayView.PassThroughTouchEvents = !_overlayCancelsTouchesInView;

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
            StartAngle = new Angle { Degrees = degrees };
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

            if (IsUnfolded)
                return this;

            IsUnfolded = true;
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
                IsUnfolded = false;
                Debug.WriteLine("ALRadialMenu has no buttons to present");
                return this;
            }

            if (IsUnfolded)
                OnDismissing?.Invoke(this, null);

            Dismiss(-1);

            return this;
        }

        private void InternalDismiss() => Dismiss();

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
            if (PresentButtonAnimation != null)
            {
                PresentButtonAnimation.Invoke(view, index);
                return;
            }

            var degrees = StartAngle.Degrees + Spacing.Degrees * index;
            var newCenter = PointOnCircumference(_animationOrigin, _radius, new Angle { Degrees = degrees });
            var delay = index * _delay;

            view.Center = _animationOrigin;
            view.Alpha = 0;
            AnimateNotify(0.5, delay, 0.7f, 0.7f, AnimationOptions, () =>
            {
                view.Alpha = 1;
                view.Center = newCenter;
            }, finished =>
            {
            });
        }

        private void DismissAnimation(UIView view, int index)
        {
            if (DismissButtonAnimation != null)
            {
                DismissButtonAnimation.Invoke(view, index);
                return;
            }

            var delay = index * _delay;

            AnimateNotify(0.5, delay, 0.7f, 0.7f, AnimationOptions,
                () =>
                {
                    view.Alpha = 0;
                    view.Center = _animationOrigin;
                }, finished =>
                {
                    view.RemoveFromSuperview();
                    if (index == Buttons.Count - 1)
                        IsUnfolded = false;
                });
        }

        private void SelectedAnimation(UIView view)
        {
            if (SelectedButtonAnimation != null)
            {
                SelectedButtonAnimation.Invoke(view);
                return;
            }

            AnimateNotify(0.5, 0, 0.7f, 0.7f, AnimationOptions,
                () =>
                {
                    view.Alpha = 0;
                    view.Transform = CGAffineTransform.MakeScale(1.5f, 1.5f);
                }, finished =>
                {
                    view.Transform = CGAffineTransform.MakeIdentity();
                    view.RemoveFromSuperview();
                });
        }

        private void OverlayView_OnOverlayPointInside(CGPoint point, UIEvent uievent)
        {
            if (_dismissOnOverlayTap)
                InternalDismiss();
        }

        private static CGPoint PointOnCircumference(CGPoint origin, float radius, Angle angle)
        {
            var radians = angle.Radians;
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

            Spacing = new Angle { Degrees = Circumference.Degrees / count };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _orientationToken?.Dispose();

                if (_overlayView != null)
                {
                    _overlayView.RemoveFromSuperview();
                    _overlayView.RemoveGestureRecognizer(_dismissGesture);
                    _overlayView.Dispose();
                }

                _dismissGesture?.Dispose();

                foreach (var button in Buttons.ToArray())
                {
                    if (button.Superview != null)
                        button.RemoveFromSuperview();

                    button.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
