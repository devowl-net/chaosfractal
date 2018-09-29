using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

using CF.Application.Controls.Views;

namespace CF.Application.Windows.Behaviors
{
    /// <summary>
    /// Set point on mouse click behavior.
    /// </summary>
    public class SetPointBehavior : Behavior<ChaosField>
    {
        /// <summary>
        /// Dependency property for <see cref="IsEditMode"/>.
        /// </summary>
        public static readonly DependencyProperty IsEditModeProperty;

        static SetPointBehavior()
        {
            IsEditModeProperty = DependencyProperty.Register(
                nameof(IsEditMode),
                typeof(bool),
                typeof(SetPointBehavior),
                new PropertyMetadata(default(bool)));


        }

        /// <summary>
        /// Constructor for <see cref="SetPointBehavior"/>.
        /// </summary>
        public SetPointBehavior()
        {
            
        }

        /// <summary>
        /// Edit mode activated.
        /// </summary>
        public bool IsEditMode
        {
            get
            {
                return (bool)GetValue(IsEditModeProperty);
            }

            set
            {
                SetValue(IsEditModeProperty, value);
            }
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonUp += OnLeftButtonUp;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonUp -= OnLeftButtonUp;
        }

        private void SetAnchorPointHandler(Point point)
        {
            AssociatedObject.SetAnchorPoint(point);
        }

        private void OnLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Released && IsEditMode)
            {
                var position = args.GetPosition(AssociatedObject);
                AssociatedObject.SetAnchorPoint(position);
            }
        }
    }
}