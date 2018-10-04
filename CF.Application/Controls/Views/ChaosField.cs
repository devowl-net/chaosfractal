using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using CF.Application.Common;
using CF.Application.Controls.Data;
using CF.Application.Controls.Models;

namespace CF.Application.Controls.Views
{
    /// <summary>
    /// Chaos field control.
    /// </summary>
    public class ChaosField : StackPanel
    {
        private const int BackgroundGridZIndex = 0;

        private readonly IDictionary<DotType, int> _dotTypeZIndexes = new Dictionary<DotType, int>()
        {
            { DotType.Track, 1 },
            { DotType.CurrentTrack, 2 },
            { DotType.Random, 3 },
            { DotType.Anchor, 4 },
        };

        private PointData _randomPoint;

        private PointData _currentTrackPoint;

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
        
        private readonly ICollection<PointData> _anchorPoints = new List<PointData>();

        private readonly ICollection<PointData> _trackPoints = new List<PointData>();

        private readonly ICollection<Line> _lines = new List<Line>();

        /// <summary>
        /// Dependency property for <see cref="ChaosManager"/> property.
        /// </summary>
        public static readonly DependencyProperty ChaosManagerProperty;

        /// <summary>
        /// Chaos manager instance.
        /// </summary>
        public ChaosManager ChaosManager
        {
            get
            {
                return (ChaosManager)GetValue(ChaosManagerProperty);
            }

            set
            {
                throw new InvalidOperationException("Please use OneWayToSource binding mode");
            }
        }
        
        static ChaosField()
        {
            ChaosManagerProperty = DependencyProperty.Register(
                nameof(ChaosManager),
                typeof(ChaosManager),
                typeof(ChaosField));

            DotRadiusProperty = DependencyProperty.Register(
                nameof(DotRadius),
                typeof(double),
                typeof(ChaosField),
                new PropertyMetadata(5.0));

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
            GameLogic = new GameLogic(this);
        }

        /// <summary>
        /// User random point.
        /// </summary>
        public Point? RandomPoint => _randomPoint?.Point;

        /// <summary>
        /// User random point.
        /// </summary>
        public Point? CurrentTrackPoint => _currentTrackPoint?.Point;

        /// <summary>
        /// Current anchor point roster.
        /// </summary>
        public IDictionary<Point, Brush> AnchorPoints
        {
            get
            {
                return _anchorPoints.ToDictionary(data => data.Point, data => data.Color);
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

                buffer = charName1.ToString();
            }

            throw new InvalidOperationException("Too many dots");
        }

        private static Line CreateLine(int x1, int y1, int x2, int y2)
        {
            return new Line { X1 = x1, X2 = x2, Y1 = y1, Y2 = y2, StrokeThickness = 1 };
        }

        /// <summary>
        /// When point added event.
        /// </summary>
        public event EventHandler<PointArgs> OnPointAdded;

        /// <summary>
        /// Set anchor point.
        /// </summary>
        /// <param name="point">Point coordinates.</param>
        /// <param name="dotType">Dot type.</param>
        /// <param name="color">Цвет рисуемой точки.</param>
        public void DrawPoint(Point point, DotType dotType, Brush color = null)
        {
            switch (dotType)
            {
                case DotType.Anchor:
                    DrawAnchor(point);
                    break;
                case DotType.CurrentTrack:
                    DrawCurrentTrack(point);
                    break;
                case DotType.Random:
                    DrawRandom(point);
                    break;
                case DotType.Track:
                    DrawTrack(point, color);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dotType), dotType, null);
            }
        }

        private void DrawCurrentTrack(Point point)
        {
            _currentTrackPoint?.Clear();
            _currentTrackPoint = new PointData(CanvasRef) { Point = point, PointVisual = DrawPointObject(point, Brushes.Yellow, DotType.CurrentTrack) };
        }

        public GameLogic GameLogic { get; }

        private void DrawTrack(Point point, Brush color)
        {
            if (_trackPoints.Any(t => t.Point == point))
            {
                return;
            }

            var pointData = new PointData(CanvasRef)
            {
                Point = point,
                Color = color
            };

            _trackPoints.Add(pointData);
            pointData.PointVisual  = DrawPointObject(point, color, DotType.Track);
            OnPointAdded?.Invoke(this, new PointArgs(point, DotType.Track));
        }

        private void DrawRandom(Point point)
        {
            _randomPoint?.Clear();
            _randomPoint = new PointData(CanvasRef)
            {
                Point = point,
                PointVisual = DrawPointObject(point, Brushes.LawnGreen, DotType.Random)
            };

            OnPointAdded?.Invoke(this, new PointArgs(point, DotType.Random));
        }

        private void DrawAnchor(Point point)
        {
            if (_anchorPoints.Any(a => a.Point == point))
            {
                return;
            }

            var name = GetFreePointName(_anchorPoints.Select(data => data.Name));
            var color = SolidBrushProvider.GetNextColor();
            var pointData = new PointData(CanvasRef)
            {
                Name = name,
                Point = point,
                Color = color
            };

            _anchorPoints.Add(pointData);
            pointData.PointVisual = DrawPointObject(point, color, DotType.Anchor);
            pointData.NameVisual = DrawTextObject(point, name, DotType.Anchor);
            OnPointAdded?.Invoke(this, new PointArgs(point, DotType.Anchor));
        }

        private UIElement DrawTextObject(Point point, string text, DotType dotType)
        {
            var nameBlock = new TextBlock();
            CanvasRef.Children.Add(nameBlock);
            nameBlock.Text = text;
            nameBlock.Foreground = Brushes.White;
            Canvas.SetLeft(nameBlock, point.X + DotRadius);
            Canvas.SetTop(nameBlock, point.Y);
            SetZIndex(nameBlock, _dotTypeZIndexes[dotType]);

            return nameBlock;
        }

        private UIElement DrawPointObject(Point point, Brush color, DotType dotType)
        {
            var dot = new Ellipse() { Width = DotRadius, Height = DotRadius, Fill = color };
            
            CanvasRef.Children.Add(dot);
            Canvas.SetLeft(dot, point.X);
            Canvas.SetTop(dot, point.Y);
            SetZIndex(dot, _dotTypeZIndexes[dotType]);

            return dot;
        }

        private void ControlLoaded(object sender, RoutedEventArgs args)
        {
            Loaded -= ControlLoaded;
            SetValue(ChaosManagerProperty, new ChaosManager(this));
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
                SetZIndex(horizontalLine, BackgroundGridZIndex);
                _lines.Add(horizontalLine);
            }

            // Columns
            for (int j = 0; j <= width; j++)
            {
                var verticalLine = CreateLine(j * CellSize, 0, j * CellSize, height * CellSize);
                verticalLine.Stroke = j % 10 == 0 ? LineBrush : tenthLine;
                CanvasRef.Children.Add(verticalLine);
                SetZIndex(verticalLine, BackgroundGridZIndex);
                _lines.Add(verticalLine);
            }
        }

        private class PointData
        {
            private readonly Panel _panel;

            /// <summary>
            /// Constructor for <see cref="PointData"/>.
            /// </summary>
            public PointData(Panel panel)
            {
                if (panel == null)
                {
                    throw new ArgumentNullException(nameof(panel));
                }

                _panel = panel;
            }

            /// <summary>
            /// Point coordinate.
            /// </summary>
            public Point Point { get; set; }

            /// <summary>
            /// Point color.
            /// </summary>
            public Brush Color { get; set; }

            /// <summary>
            /// Visual presentation on the screen.
            /// </summary>
            public UIElement PointVisual { get; set; }

            /// <summary>
            /// Point name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// PointVisual text on the screen.
            /// </summary>
            public UIElement NameVisual { get; set; }

            /// <summary>
            /// Clear point data.
            /// </summary>
            public void Clear()
            {
                new List<UIElement>
                {
                    PointVisual,
                    NameVisual
                }.ForEach(_panel.Children.Remove);
            }
        }

        /// <summary>
        /// Clear game field.
        /// </summary>
        public void Clear()
        {
            foreach (var point in _anchorPoints)
            {
                point.Clear();
            }

            _currentTrackPoint.Clear();
            _randomPoint.Clear();
            foreach (var point in _trackPoints)
            {
                point.Clear();
            }

            _trackPoints.Clear();
            _anchorPoints.Clear();
            _currentTrackPoint = _randomPoint = null;
        }
    }
}