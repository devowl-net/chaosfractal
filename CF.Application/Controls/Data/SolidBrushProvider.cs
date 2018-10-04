using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace CF.Application.Controls.Data
{
    /// <summary>
    /// <see cref="Brush"/> color provider.
    /// </summary>
    public static class SolidBrushProvider
    {
        private static readonly ReadOnlyCollection<SolidColorBrush> Colors;

        private static readonly Random Random = new Random();

        private static readonly IEnumerable<string> BadColors = new[]
        {
            nameof(Brushes.Black),
            nameof(Brushes.DarkBlue),
            nameof(Brushes.Indigo),
            nameof(Brushes.Navy),
            nameof(Brushes.MidnightBlue),
            nameof(Brushes.MediumBlue),
        };

        static SolidBrushProvider()
        {
            Colors =
                new ReadOnlyCollection<SolidColorBrush>(
                    typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static)
                        .Where(property => !BadColors.Contains(property.Name))
                        .Select(property => (SolidColorBrush)property.GetValue(null, null))
                        .ToList());
        }

        /// <summary>
        /// Get next random color.
        /// </summary>
        /// <returns>Random color.</returns>
        public static SolidColorBrush GetNextColor()
        {
            return Colors[Random.Next(Colors.Count)];
        }
    }
}