using System;
using System.Windows;

using CF.Application.Common;

namespace CF.Application.Controls.Data
{
    /// <summary>
    /// Point arguments.
    /// </summary>
    public class PointArgs : EventArgs
    {
        /// <summary>
        /// Constructor for <see cref="PointArgs"/>.
        /// </summary>
        public PointArgs(Point point, DotType dotType)
        {
            Point = point;
            DotType = dotType;
        }

        /// <summary>
        /// Dot type.
        /// </summary>
        public DotType DotType { get; }

        /// <summary>
        /// Point coordinate.
        /// </summary>
        public Point Point { get; }
    }
}