using System.Windows;

using CF.Application.Common;
using CF.Application.Controls.Models;
using CF.Application.Prism;
using CF.Application.Windows.Views;

namespace CF.Application.Windows.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="MainWindow"/>.
    /// </summary>
    public class MainWindowViewModel : NotificationObject
    {
        private ChaosManager _chaosManager;

        /// <summary>
        /// Constructor for <see cref="MainWindowViewModel"/>.
        /// </summary>
        public MainWindowViewModel()
        {
        }

        /// <summary>
        /// Chaos field manager instance.
        /// </summary>
        public ChaosManager ChaosManager
        {
            get
            {
                return _chaosManager;
            }

            set
            {
                if (value != null)
                {
                    _chaosManager = value;
                    _chaosManager.SetHandler(PointTypeManager);
                }
            }
        }

        private DotType PointTypeManager(Point point)
        {
            return DotType.Anchor;
        }
    }
}