using System;
using System.Collections.Generic;
using System.Windows;

using CF.Application.Common;
using CF.Application.Controls.Data;
using CF.Application.Controls.Views;

namespace CF.Application.Controls.Models
{
    /// <summary>
    /// Chaos game field external game manager.
    /// </summary>
    public class ChaosManager
    {
        private readonly ChaosField _chaosField;

        private Func<Point, DotType> _handler;

        /// <summary>
        /// When point added event.
        /// </summary>
        public event EventHandler<PointArgs> OnPointAdded;

        /// <summary>
        /// Constructor for <see cref="ChaosManager"/>.
        /// </summary>
        public ChaosManager(ChaosField chaosField)
        {
            if (chaosField == null)
            {
                throw new ArgumentNullException(nameof(chaosField));
            }

            _chaosField = chaosField;
            _chaosField.OnPointAdded += (sender, args) => OnPointAdded?.Invoke(sender, args);
        }

        /// <summary>
        /// Set mouse click handler.
        /// </summary>
        /// <param name="handler"></param>
        public void SetHandler(Func<Point, DotType> handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Anchor points.
        /// </summary>
        public IEnumerable<Point> PressedAnchors => _chaosField.AnchorPoints.Keys;

        /// <summary>
        /// Random point coordinate.
        /// </summary>
        public Point? Random => _chaosField.RandomPoint;

        /// <summary>
        /// Mouse pressed.
        /// </summary>
        /// <param name="position">Mouse click position.</param>
        public void MousePressed(Point position)
        {
            if (_handler != null)
            {
                _chaosField.DrawPoint(position, _handler(position));
            }
        }

        /// <summary>
        /// Start game.
        /// </summary>
        /// <param name="factor">Distance factor.</param>
        public void Start(int factor)
        {
            _chaosField.GameLogic.StartGame(factor);
        }

        /// <summary>
        /// Reset game.
        /// </summary>
        public void Reset()
        {
            _chaosField.GameLogic.StopGame();
            _chaosField.Clear();
        }
    }
}