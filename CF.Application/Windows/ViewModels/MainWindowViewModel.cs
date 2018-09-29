using System;
using System.Windows;

using CF.Application.Prism;
using CF.Application.Windows.Views;

namespace CF.Application.Windows.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="MainWindow"/>.
    /// </summary>
    public class MainWindowViewModel : NotificationObject
    {
        /// <summary>
        /// Constructor for <see cref="MainWindowViewModel"/>.
        /// </summary>
        public MainWindowViewModel()
        {
            
        }

        /// <summary>
        /// Set anchor point.
        /// </summary>
        public Action<Point> SetAnchorPoint { get; set; }
    }
}