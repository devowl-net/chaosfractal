using System.Collections.Generic;
using System.Linq;
using System.Windows;

using CF.Application.Common;
using CF.Application.Controls.Data;
using CF.Application.Controls.Models;
using CF.Application.Prism;
using CF.Application.Windows.Models;
using CF.Application.Windows.Views;

namespace CF.Application.Windows.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="MainWindow"/>.
    /// </summary>
    public class MainWindowViewModel : NotificationObject
    {
        private ChaosManager _chaosManager;

        private bool? _step2Passed;

        private bool? _step1Passed;

        /// <summary>
        /// Constructor for <see cref="MainWindowViewModel"/>.
        /// </summary>
        public MainWindowViewModel()
        {
            _step2Passed = false;
            Step1Command = new DelegateCommand(
                OnStep1Click,
                o => ChaosManager != null && ChaosManager.PressedAnchors.Count() >= 3);
            Step2Command = new DelegateCommand(OnStep2Click, o => ChaosManager?.Random != null);
            ResetCommand = new DelegateCommand(ResetGame);

            // http://unicodefractions.com/
            Distances = new[]
            {
                new Distance(189, 2),
                new Distance(8531, 3),
                new Distance(188, 4),
                new Distance(8533, 5),
                new Distance(8537, 6),
            };
        }

        /// <summary>
        /// Step 2 passed trigger.
        /// </summary>
        public bool? Step2Passed
        {
            get
            {
                return _step2Passed;
            }

            set
            {
                _step2Passed = value;
                RaisePropertyChanged(() => Step2Passed);
            }
        }

        /// <summary>
        /// Step 1 passed trigger.
        /// </summary>
        public bool? Step1Passed
        {
            get
            {
                return _step1Passed;
            }

            set
            {
                _step1Passed = value;
                RaisePropertyChanged(() => Step1Passed);
            }
        }

        /// <summary>
        /// Distance factor.
        /// </summary>
        public IEnumerable<Distance> Distances { get; }

        /// <summary>
        /// Selected distance factor.
        /// </summary>
        public Distance SelectedFactor { get; set; }

        /// <summary>
        /// Reset game command.
        /// </summary>
        public DelegateCommand ResetCommand { get; }

        /// <summary>
        /// Step2 okay button.
        /// </summary>
        public DelegateCommand Step2Command { get; }

        /// <summary>
        /// Step1 okay button.
        /// </summary>
        public DelegateCommand Step1Command { get; }

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
                    _chaosManager.OnPointAdded -= OnOnPointAdded;
                    _chaosManager.OnPointAdded += OnOnPointAdded;
                }
            }
        }

        private void ResetGame(object obj)
        {
            Step1Passed = null;
            Step2Passed = false;
            ChaosManager.Reset();
        }

        private void OnStep2Click(object obj)
        {
            Step2Passed = true;
            ChaosManager.Start(SelectedFactor.Value);
        }

        private void OnStep1Click(object obj)
        {
            Step1Passed = true;
            Step2Passed = null;
        }

        private void OnOnPointAdded(object sender, PointArgs args)
        {
            Step1Command.RaiseCanExecuteChanged();
            Step2Command.RaiseCanExecuteChanged();
        }

        private DotType PointTypeManager(Point point)
        {
            if (Step1Passed == null)
            {
                return DotType.Anchor;
            }

            if (Step2Passed == null)
            {
                return DotType.Random;
            }

            return DotType.Track;
        }
    }
}