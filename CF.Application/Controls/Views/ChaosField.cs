using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CF.Application.Controls.Views
{
    /// <summary>
    /// Chaos field control.
    /// </summary>
    public class ChaosField : StackPanel
    {
        /// <summary>
        /// Dependency property for <see cref="LineBrush"/> property.
        /// </summary>
        public static readonly DependencyProperty LineBrushProperty;

        /// <summary>
        /// Dependency property for <see cref="CellSize"/> property.
        /// </summary>
        public static readonly DependencyProperty CellSizeProperty;

        /// <summary>
        /// Dependency property for <see cref="DotRadius"/> property.
        /// </summary>
        public static readonly DependencyProperty DotRadiusProperty;

        /// <summary>
        /// Dependency property for <see cref="SetAnchorPoint"/>.
        /// </summary>
        public static readonly DependencyProperty SetAnchorPointProperty;

        private readonly IDictionary<Point, PointData> _anchorPoints = new Dictionary<Point, PointData>();

        private readonly ICollection<Line> _lines = new List<Line>();

        static ChaosField()
        {
            SetAnchorPointProperty = DependencyProperty.Register(
                nameof(SetAnchorPoint),
                typeof(Action<Point>),
                typeof(ChaosField));

            DotRadiusProperty = DependencyProperty.Register(
                nameof(DotRadius),
                typeof(double),
                typeof(ChaosField),
                new PropertyMetadata(5));

            LineBrushProperty = DependencyProperty.Register(
                nameof(LineBrush),
                typeof(Brush),
                typeof(ChaosField),
                new PropertyMetadata(
                    new BrushConverter().ConvertFrom("#2d2d2d"),
                    (source, args) => ((ChaosField)source).SquareGrid()));

            CellSizeProperty = DependencyProperty.Register(
                nameof(CellSize),
                typeof(int),
                typeof(ChaosField),
                new PropertyMetadata(10, (source, args) => ((ChaosField)source).SquareGrid()));
        }

        /// <summary>
        /// Constructor for <see cref="ChaosField"/>.
        /// </summary>
        public ChaosField()
        {
            CanvasRef = new Canvas();
            Children.Add(CanvasRef);
            Background = Brushes.Black;
            Loaded += ControlLoaded;
            SizeChanged += (sender, args) => SquareGrid();
            SetValue(SetAnchorPointProperty, new Action<Point>(InternalSetAnchorPoint));
        }

        private enum DotType
        {
            Anchor,

            Main,

            Track
        }

        /// <summary>
        /// Current anchor point roster.
        /// </summary>
        public IEnumerable<Point> AnchorPoints
        {
            get
            {
                return _anchorPoints.Keys;
            }
        }

        /// <summary>
        /// Radius of dots on the screen.
        /// </summary>
        public double DotRadius
        {
            get
            {
                return (double)GetValue(DotRadiusProperty);
            }
            set
            {
                SetValue(DotRadiusProperty, value);
            }
        }

        /// <summary>
        /// Line brush color.
        /// </summary>
        public Brush LineBrush
        {
            get
            {
                return (Brush)GetValue(LineBrushProperty);
            }

            set
            {
                SetValue(LineBrushProperty, value);
            }
        }

        /// <summary>
        /// Cell size (pixels).
        /// </summary>
        public int CellSize
        {
            get
            {
                return (int)GetValue(CellSizeProperty);
            }

            set
            {
                SetValue(CellSizeProperty, value);
            }
        }

        /// <summary>
        /// Set archer point action.
        /// </summary>
        public Action<Point> SetAnchorPoint
        {
            get
            {
                return (Action<Point>)GetValue(SetAnchorPointProperty);
            }

            set
            {
                throw new InvalidOperationException("You can't set anchor function");
            }
        }

        private Canvas CanvasRef { get; }

        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        private static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = color.R;
            float green = color.G;
            float blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        private static string GetFreePointName(IEnumerable<string> names)
        {
            var localNames = names.ToArray();
            char[] charNames = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string buffer = string.Empty;

            foreach (var charName1 in charNames)
            {
                foreach (var charName2 in charNames)
                {
                    var newName = buffer + charName2;
                    if (!localNames.Contains(newName))
                    {
                        return newName;
                    }
                }

                buffer += charName1;
            }

            throw new InvalidOperationException("Too many dots");
        }

        private static Line CreateLine(int x1, int y1, int x2, int y2)
        {
            return new Line { X1 = x1, X2 = x2, Y1 = y1, Y2 = y2, StrokeThickness = 1 };
        }

        /// <summary>
        /// Set anchor point.
        /// </summary>
        /// <param name="point"></param>
        private void InternalSetAnchorPoint(Point point)
        {
            if (_anchorPoints.ContainsKey(point))
            {
                return;
            }

            var pointName = GetFreePointName(_anchorPoints.Values.Select(data => data.VisualName));
            var pointData = new PointData() { VisualName = pointName };
            _anchorPoints.Add(point, pointData);
            DrawPoint(pointData, DotType.Anchor);
        }

        private void DrawPoint(PointData pointData, DotType dotType)
        {
            Brush dotBrush;
            switch (dotType)
            {
                case DotType.Anchor:
                    dotBrush = Brushes.DeepSkyBlue;
                    break;
                case DotType.Main:
                    dotBrush = Brushes.LawnGreen;
                    break;
                case DotType.Track:
                    dotBrush = Brushes.Wheat;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dotType), dotType, null);
            }

            var dot = new Ellipse() { Width = DotRadius, Height = DotRadius, Fill = dotBrush, Tag = pointData };

            CanvasRef.Children.Add(dot);
            Canvas.SetLeft(dot, pointData.Coordinate.X);
            Canvas.SetTop(dot, pointData.Coordinate.Y);

            if (dotType == DotType.Anchor)
            {
            }
        }

        private void ControlLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= ControlLoaded;
            SquareGrid();
        }

        private void SquareGrid()
        {
            foreach (var line in _lines.ToArray())
            {
                CanvasRef.Children.Remove(line);
                _lines.Remove(line);
            }

            var mainColor = LineBrush;
            var tenthLine = new SolidColorBrush(ChangeColorBrightness(((SolidColorBrush)mainColor).Color, -0.6f));

            int height = (int)(ActualHeight / CellSize);
            int width = (int)(ActualWidth / CellSize);

            // Rows
            for (int i = 0; i <= height; i++)
            {
                var horizontalLine = CreateLine(0, i * CellSize, width * CellSize, i * CellSize);
                horizontalLine.Stroke = i % 10 == 0 ? LineBrush : tenthLine;
                CanvasRef.Children.Add(horizontalLine);
                _lines.Add(horizontalLine);
            }

            // Columns
            for (int j = 0; j <= width; j++)
            {
                var verticalLine = CreateLine(j * CellSize, 0, j * CellSize, height * CellSize);
                verticalLine.Stroke = j % 10 == 0 ? LineBrush : tenthLine;
                CanvasRef.Children.Add(verticalLine);
                _lines.Add(verticalLine);
            }
        }

        private class PointData
        {
            /// <summary>
            /// Point coordinate.
            /// </summary>
            public Point Coordinate { get; set; }

            /// <summary>
            /// Visual presentation on the screen.
            /// </summary>
            public Ellipse Visual { get; set; }

            /// <summary>
            /// Visual name.
            /// </summary>
            public string VisualName { get; set; }
        }
    }
}