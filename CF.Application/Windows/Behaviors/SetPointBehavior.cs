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
        
        private void OnLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Released)
            {
                var position = args.GetPosition(AssociatedObject);
                AssociatedObject.ChaosManager.MousePressed(position);
            }
        }
    }
}