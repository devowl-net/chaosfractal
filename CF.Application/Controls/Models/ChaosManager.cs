using System;
using System.Windows;

using CF.Application.Common;
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
        /// Constructor for <see cref="ChaosManager"/>.
        /// </summary>
        public ChaosManager(ChaosField chaosField)
        {
            _chaosField = chaosField;
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
        /// Mouse pressed.
        /// </summary>
        /// <param name="position"></param>
        public void MousePressed(Point position)
        {
            if (_handler != null)
            {
                _chaosField.DrawPoint(position, _handler(position));
            }
        }
    }
}