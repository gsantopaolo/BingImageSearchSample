using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoAlbum.Controls
{
    /// <summary>
    /// Thin wrapper around the <see cref="Windows.UI.Input.GestureRecognizer"/>, routes pointer events received by
    /// the manipulation target to the gesture recognizer.
    /// </summary>
    /// <remarks>
    /// Transformations during manipulations cannot be expressed in the coordinate space of the manipulation target.
    /// Instead they need be expressed with respect to a reference coordinate space, usually an ancestor (in the UI tree)
    /// of the element being manipulated.
    /// </remarks>
    public abstract class InputProcessor
    {
        protected Windows.UI.Input.GestureRecognizer _gestureRecognizer;

        // Element being manipulated
        protected Windows.UI.Xaml.FrameworkElement _target;
        public Windows.UI.Xaml.FrameworkElement Target
        {
            get { return _target; }
        }

        // Reference element that contains the coordinate space used for expressing transformations 
        // during manipulation, usually the parent element of Target in the UI tree
        protected Windows.UI.Xaml.Controls.Canvas _reference;
        public Windows.UI.Xaml.FrameworkElement Reference
        {
            get { return _reference; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">
        /// Manipulation target.
        /// </param>
        /// <param name="reference">
        /// Element that contains the coordinate space used for expressing transformations
        /// during manipulations, usually the parent element of Target in the UI tree.
        /// </param>
        /// <remarks>
        /// Transformations during manipulations cannot be expressed in the coordinate space of the manipulation target.
        /// Thus <paramref name="element"/> and <paramref name="reference"/> must be different. Usually <paramref name="reference"/>
        /// will be an ancestor of <paramref name="element"/> in the UI tree.
        /// </remarks>
        internal InputProcessor(Windows.UI.Xaml.FrameworkElement element, Windows.UI.Xaml.Controls.Canvas reference)
        {
            _target = element;
            _reference = reference;

            // Setup pointer event handlers for the element.
            // They are used to feed the gesture recognizer.    
            _target.PointerCanceled += OnPointerCanceled;
            _target.PointerMoved += OnPointerMoved;
            _target.PointerPressed += OnPointerPressed;
            _target.PointerReleased += OnPointerReleased;
            _target.PointerWheelChanged += OnPointerWheelChanged;

            // Create the gesture recognizer
            _gestureRecognizer = new Windows.UI.Input.GestureRecognizer();
            _gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.None;
        }

        #region Pointer event handlers

        private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            //_target.Opacity = 0.5;
            int index = Windows.UI.Xaml.Controls.Canvas.GetZIndex(_target);
            int total = _reference.Children.Count;
            Windows.UI.Xaml.Controls.Canvas.SetZIndex(_target, total - 1);

            // Obtain current point in the coordinate system of the reference element
            Windows.UI.Input.PointerPoint currentPoint = args.GetCurrentPoint(_reference);

            // Route the event to the gesture recognizer
            _gestureRecognizer.ProcessDownEvent(currentPoint);

            // Capture the pointer associated to this event
            _target.CapturePointer(args.Pointer);

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
        }

        private void OnPointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            // Route the events to the gesture recognizer.
            // All intermediate points are passed to the gesture recognizer in
            // the coordinate system of the reference element.
            _gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(_reference));

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
        }

        private void OnPointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            // Obtain current point in the coordinate system of the reference element
            Windows.UI.Input.PointerPoint currentPoint = args.GetCurrentPoint(_reference);

            // Route the event to the gesture recognizer
            _gestureRecognizer.ProcessUpEvent(currentPoint);

            // Release pointer capture on the pointer associated to this event
            _target.ReleasePointerCapture(args.Pointer);

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
        }

        private void OnPointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            // Obtain current point in the coordinate system of the reference element
            Windows.UI.Input.PointerPoint currentPoint = args.GetCurrentPoint(_reference);

            // Find out whether shift/ctrl buttons are pressed
            bool shift = (args.KeyModifiers & Windows.System.VirtualKeyModifiers.Shift) == Windows.System.VirtualKeyModifiers.Shift;
            bool ctrl = (args.KeyModifiers & Windows.System.VirtualKeyModifiers.Control) == Windows.System.VirtualKeyModifiers.Control;

            // Route the event to the gesture recognizer
            _gestureRecognizer.ProcessMouseWheelEvent(currentPoint, shift, ctrl);

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
        }

        private void OnPointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs args)
        {
            _gestureRecognizer.CompleteGesture();

            // Release pointer capture on the pointer associated to this event
            _target.ReleasePointerCapture(args.Pointer);

            // Mark event handled, to prevent execution of default event handlers
            args.Handled = true;
        }

        #endregion Pointer event handlers
    }

    /// <summary>
    /// Provides implementations of the <see cref="FilterManipulation"/> delegate that are useful in this sample.
    /// </summary>
    public class ManipulationFilter
    {
        private static float TargetMinSize = 100F;
        private static float TargetMaxSize = 10000F;
        private static float TargetMinInside = 50F;

        /// <summary>
        /// Implementation of <see cref="FilterManipulation"/> that forces the rotation to be about
        /// the center of the manipulation target.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void RotateAboutCenter(object sender, FilterManipulationEventArgs args)
        {
            var inputProcessor = sender as InputProcessor;
            var target = inputProcessor.Target;

            // Get the bounding box of the manipulation target, expressed in the coordinate system of its container
            var rect = target.RenderTransform.TransformBounds(
                new Windows.Foundation.Rect(0, 0, target.ActualWidth, target.ActualHeight));

            args.Pivot = new Windows.Foundation.Point
            {
                X = rect.Left + (rect.Width / 2),
                Y = rect.Top + (rect.Height / 2)
            };
        }

        /// <summary>
        /// Implementation of <see cref="FilterManipulation"/> that forces at least <see cref="ManipulationFilter.TargetMinSize"/>
        /// pixels of the manipulation target to remain inside its container.
        /// This filter also makes sure the manipulation target does not become too small or too big.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void Clamp(object sender, FilterManipulationEventArgs args)
        {
            var inputProcessor = sender as InputProcessor;
            var target = inputProcessor.Target;
            var container = inputProcessor.Reference;

            // Get the bounding box of the manipulation target, expressed in the coordinate system of its container
            var rect = target.RenderTransform.TransformBounds(
                new Windows.Foundation.Rect(0, 0, target.ActualWidth, target.ActualHeight));

            // Make sure the manipulation target does not go completely outside the boundaries of its container
            var translate = new Windows.Foundation.Point
            {
                X = args.Delta.Translation.X,
                Y = args.Delta.Translation.Y
            };
            if ((args.Delta.Translation.X > 0 && args.Delta.Translation.X > container.ActualWidth - rect.Left - ManipulationFilter.TargetMinInside) ||
                (args.Delta.Translation.X < 0 && args.Delta.Translation.X < ManipulationFilter.TargetMinInside - rect.Right) ||
                (args.Delta.Translation.Y > 0 && args.Delta.Translation.Y > container.ActualHeight - rect.Top - ManipulationFilter.TargetMinInside) ||
                (args.Delta.Translation.Y < 0 && args.Delta.Translation.Y < ManipulationFilter.TargetMinInside - rect.Bottom))
            {
                translate.X = 0;
                translate.Y = 0;
            }

            // Make sure the manipulation target does not become too small, or too big
            float scale = args.Delta.Scale < 1F ?
                (float)System.Math.Max(ManipulationFilter.TargetMinSize / System.Math.Min(rect.Width, rect.Height), args.Delta.Scale) :
                (float)System.Math.Min(ManipulationFilter.TargetMaxSize / System.Math.Max(rect.Width, rect.Height), args.Delta.Scale);

            args.Delta = new Windows.UI.Input.ManipulationDelta
            {
                Expansion = args.Delta.Expansion,
                Rotation = args.Delta.Rotation,
                Scale = scale,
                Translation = translate
            };
        }

        /// <summary>
        /// Implementation of <see cref="FilterManipulation"/> that forces the center of mass of the
        /// manipulation target to remain inside its container.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void ClampCenterOfMass(object sender, FilterManipulationEventArgs args)
        {
            var inputProcessor = sender as InputProcessor;
            var target = inputProcessor.Target;
            var container = inputProcessor.Reference;

            // Get the bounding box and the center of mass of the manipulation target, 
            // expressed in the coordinate system of its container
            var rect = target.RenderTransform.TransformBounds(
                new Windows.Foundation.Rect(0, 0, target.ActualWidth, target.ActualHeight));

            var centerOfMass = new Windows.Foundation.Point
            {
                X = rect.Left + (rect.Width / 2),
                Y = rect.Top + (rect.Height / 2)
            };

            // Apply delta transform to the center of mass
            var transform = new Windows.UI.Xaml.Media.CompositeTransform
            {
                CenterX = args.Pivot.X,
                CenterY = args.Pivot.Y,
                Rotation = args.Delta.Rotation,
                ScaleX = args.Delta.Scale,
                ScaleY = args.Delta.Scale,
                TranslateX = args.Delta.Translation.X,
                TranslateY = args.Delta.Translation.Y
            };

            var transformedCenterOfMass = transform.TransformPoint(centerOfMass);

            // Reset the transformation if the transformed center of mass falls outside the container
            if (transformedCenterOfMass.X < 0 || transformedCenterOfMass.X > container.ActualWidth ||
                transformedCenterOfMass.Y < 0 || transformedCenterOfMass.Y > container.ActualHeight)
            {
                args.Delta = new Windows.UI.Input.ManipulationDelta
                {
                    Rotation = 0F,
                    Scale = 1F,
                    Translation = new Windows.Foundation.Point(0, 0)
                };
            }
        }
    }
    #region FilterManipulation

    public delegate void FilterManipulation(object sender, FilterManipulationEventArgs args);

    public class FilterManipulationEventArgs
    {
        internal FilterManipulationEventArgs(Windows.UI.Input.ManipulationUpdatedEventArgs args)
        {
            Delta = args.Delta;
            Pivot = args.Position;
        }

        public Windows.UI.Input.ManipulationDelta Delta
        {
            get;
            set;
        }

        public Windows.Foundation.Point Pivot
        {
            get;
            set;
        }
    }

    #endregion

    public sealed class ManipulationManager : InputProcessor
    {
        private bool _handlersRegistered;

        private Windows.UI.Xaml.Media.MatrixTransform _initialTransform;
        private Windows.UI.Xaml.Media.MatrixTransform _previousTransform;
        private Windows.UI.Xaml.Media.CompositeTransform _deltaTransform;
        private Windows.UI.Xaml.Media.TransformGroup _transform;

        /// <summary>
        /// Gets or sets the filter that is applied to each manipulation update.
        /// </summary>
        /// <remarks>
        /// OnFilterManipulation is called every time the manipulation is updated, before the update is applied.
        /// The filter can change the manipulation's pivots and deltas and it can be used for example to force
        /// the manipulated object to remain inside a region, or to make sure the scaling factor does not become
        /// too big or too small.
        /// </remarks>
        /// <seealso cref="ManipulationFilter"/>
        public FilterManipulation OnFilterManipulation
        {
            get;
            set;
        }

        public ManipulationManager(Windows.UI.Xaml.FrameworkElement element, Windows.UI.Xaml.Controls.Canvas parent)
            : base(element, parent)
        {
            _handlersRegistered = false;
            InitialTransform = _target.RenderTransform;
            ResetManipulation();
        }

        public Windows.UI.Xaml.Media.Transform InitialTransform
        {
            get { return _initialTransform; }
            set
            {
                // Save initial transform, for resetting
                _initialTransform = new Windows.UI.Xaml.Media.MatrixTransform()
                {
                    Matrix = new Windows.UI.Xaml.Media.TransformGroup()
                    {
                        Children = { value }
                    }.Value
                };
            }
        }

        /// <summary>
        /// Configures the manipulations that are enabled.
        /// </summary>
        /// <param name="scale">Boolean value that indicates if the manipulation target can be scaled.</param>
        /// <param name="rotate">Boolean value that indicates if the manipulation target can be rotated.</param>
        /// <param name="translate">Boolean value that indicates if the manipulation target can be translated.</param>
        /// <param name="inertia">Boolean value that indicates if manipulation inertia is enabled after rotate/translate.</param>
        public void Configure(bool scale, bool rotate, bool translate, bool inertia)
        {
            var settings = new Windows.UI.Input.GestureSettings();

            if (scale)
            {
                settings |= Windows.UI.Input.GestureSettings.ManipulationScale;
                if (inertia)
                {
                    settings |= Windows.UI.Input.GestureSettings.ManipulationScaleInertia;
                }
            }
            if (rotate)
            {
                settings |= Windows.UI.Input.GestureSettings.ManipulationRotate;
                if (inertia)
                {
                    settings |= Windows.UI.Input.GestureSettings.ManipulationRotateInertia;
                }
            }
            if (translate)
            {
                settings |= Windows.UI.Input.GestureSettings.ManipulationTranslateX |
                    Windows.UI.Input.GestureSettings.ManipulationTranslateY;
                if (inertia)
                {
                    settings |= Windows.UI.Input.GestureSettings.ManipulationTranslateInertia;
                }
            }
            _gestureRecognizer.GestureSettings = settings;

            ConfigureHandlers(scale || rotate || translate);
        }

        public void ResetManipulation()
        {
            // Reset previous transform to the initial transform of the element
            _previousTransform = new Windows.UI.Xaml.Media.MatrixTransform()
            {
                Matrix = _initialTransform.Matrix
            };

            // Recreate delta transfrom. This way it is initalized to the identity transform.
            _deltaTransform = new Windows.UI.Xaml.Media.CompositeTransform();

            // The actual transform is obtained as the composition of the delta transform
            // with the previous transform
            _transform = new Windows.UI.Xaml.Media.TransformGroup()
            {
                Children = { _previousTransform, _deltaTransform }
            };

            // Set the element's transform
            _target.RenderTransform = _transform;
        }

        private void OnManipulationUpdated(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.ManipulationUpdatedEventArgs args)
        {
            // Because of the way we process pointer events, all coordinates are expressed
            // in the coordinate system of the reference of the manipulation target.
            // args.Position stores the position of the pivot of this manipulation
            // args.Delta stores the deltas (Translation, Rotation in degrees, and Scale)

            var filteredArgs = new FilterManipulationEventArgs(args);
            if (OnFilterManipulation != null)
            {
                OnFilterManipulation(this, filteredArgs);
            }

            // Update the transform
            // filteredArgs.Pivot indicates the position of the pivot of this manipulation
            // filteredArgs.Delta indicates the deltas (Translation, Rotation in degrees, and Scale)
            _previousTransform.Matrix = _transform.Value;
            _deltaTransform.CenterX = filteredArgs.Pivot.X;
            _deltaTransform.CenterY = filteredArgs.Pivot.Y;
            _deltaTransform.Rotation = filteredArgs.Delta.Rotation;
            _deltaTransform.ScaleX = _deltaTransform.ScaleY = filteredArgs.Delta.Scale;
            _deltaTransform.TranslateX = filteredArgs.Delta.Translation.X;
            _deltaTransform.TranslateY = filteredArgs.Delta.Translation.Y;
        }

        private void ConfigureHandlers(bool register)
        {
            if (register && !_handlersRegistered)
            {
                _gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;

                _handlersRegistered = true;
            }
            else if (!register && _handlersRegistered)
            {
                _gestureRecognizer.ManipulationUpdated -= OnManipulationUpdated;

                _handlersRegistered = false;
            }
        }
    }
}