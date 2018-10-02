using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

using CF.Application.Common;
using CF.Application.Controls.Views;

namespace CF.Application.Controls.Data
{
    /// <summary>
    /// Game logic.
    /// </summary>
    public class GameLogic
    {
        private readonly ChaosField _chaosField;

        private readonly DispatcherTimer _dispatcherTimer;

        private Random _random;

        private Point[] _anchorPoints;

        /// <summary>
        /// Constructor for <see cref="GameLogic"/>.
        /// </summary>
        public GameLogic(ChaosField chaosField)
        {
            _chaosField = chaosField;
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += DispatcherTimerOnTick;
        }

        /// <summary>
        /// Start game.
        /// </summary>
        public void StartGame()
        {
            StopGame();
            if (_chaosField.RandomPoint == null)
            {
                return;
            }

            var userRandomPoint = _chaosField.RandomPoint.Value;
            _chaosField.DrawPoint(userRandomPoint, DotType.CurrentTrack);
            _anchorPoints = _chaosField.AnchorPoints.ToArray();
            _random = new Random();
            _dispatcherTimer.Start();
        }

        /// <summary>
        /// Stop game.
        /// </summary>
        public void StopGame()
        {
            _dispatcherTimer.Stop();
        }

        private void DispatcherTimerOnTick(object sender, EventArgs args)
        {
            if (_chaosField.CurrentTrackPoint == null)
            {
                throw new InvalidOperationException("Current track point can't be null");
            }

            var anchors = _anchorPoints.Length;
            var nextIndex = _random.Next(anchors);
            var nextAnchor = _anchorPoints[nextIndex];
            var currentTrack = _chaosField.CurrentTrackPoint.Value;
            
            var middlePoint = new Point(
                GetMiddle(nextAnchor.X, currentTrack.X),
                GetMiddle(nextAnchor.Y, currentTrack.Y));

            _chaosField.DrawPoint(currentTrack, DotType.Track);
            _chaosField.DrawPoint(middlePoint, DotType.CurrentTrack);
        }

        private double GetMiddle(double d1, double d2)
        {
            return (d2 + d1) / 2;
        }
    }
}