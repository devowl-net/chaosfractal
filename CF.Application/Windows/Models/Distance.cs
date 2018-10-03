using System;

namespace CF.Application.Windows.Models
{
    /// <summary>
    /// Distance between last track point and chosen anchor point.
    /// </summary>
    public class Distance
    {
        /// <summary>
        /// Constructor for <see cref="Distance"/>.
        /// </summary>
        public Distance(int charCode, int value)
        {
            Value = value;
            Char = char.ConvertFromUtf32(charCode);
        }

        /// <summary>
        /// Unicode char.
        /// </summary>
        public string Char { get; private set; }

        /// <summary>
        /// Faction value.
        /// </summary>
        public int Value { get; private set; }
    }
}