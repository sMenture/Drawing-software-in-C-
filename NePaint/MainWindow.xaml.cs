#region Lib
using Microsoft.Win32;
using NePaint.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
#endregion

namespace NePaint
{
    public partial class MainWindow : Window
    {
        #region variable
        private enum ToolType { Brush, Eraser }
        private ToolType currentTool = ToolType.Brush;

        private double shapeSize = 50;
        private const double MinShapeSize = 1;
        private const double MaxShapeSize = 99;

        private bool isDrawing = false;
        private Point lastPosition;
        private Point scrollStartPoint;
        private bool isScrolling = false;

        private double zoomScale = 1.0;
        private const double MinZoomScale = 0.1;
        private const double MaxZoomScale = 5.0;
        private Color selectedColor = Colors.Black;
        #endregion
        #region HotKey
        private bool InputControl()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            ShowSettingsWindow();
        }

        #region Settings
        private void ShowSettingsWindow()
        {
            WindowCreateNewCanvas settingsWindow = new WindowCreateNewCanvas();
            if (settingsWindow.ShowDialog() == true)
            {
                this.Title = $"Solution '{settingsWindow.ProjectTitle}'";
                CreateCanvas(settingsWindow.ScreenWidth, settingsWindow.ScreenHeight);
            }
        }
        private void CreateCanvas(int width, int height)
        {
            drawingCanvas.Width = width;
            drawingCanvas.Height = height;

            Rectangle border = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            drawingCanvas.Children.Add(border);

            Canvas.SetZIndex(border, int.MaxValue);
        }

        #endregion

        #region Mouse
        #region Input
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                isScrolling = true;
                scrollStartPoint = e.GetPosition(this);
                Cursor = Cursors.ScrollAll;
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDrawing = true;
                lastPosition = e.GetPosition(drawingCanvas);
                DrawShape(lastPosition);
            }
        }
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Released)
            {
                isScrolling = false;
                Cursor = Cursors.Arrow;
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                isDrawing = false;
            }
        }
        #endregion
        #region Move
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isScrolling)
            {
                Point currentPoint = e.GetPosition(this);
                double offsetX = currentPoint.X - scrollStartPoint.X;
                double offsetY = currentPoint.Y - scrollStartPoint.Y;

                double newLeft = Canvas.GetLeft(drawingCanvas) + offsetX;
                double newTop = Canvas.GetTop(drawingCanvas) + offsetY;

                TranslateTransform currentTransform = drawingCanvas.RenderTransform as TranslateTransform;
                double currentOffsetX = currentTransform != null ? currentTransform.X : 0;
                double currentOffsetY = currentTransform != null ? currentTransform.Y : 0;

                // Обновляем значения смещения
                double newOffsetX = currentOffsetX + offsetX;
                double newOffsetY = currentOffsetY + offsetY;

                drawingCanvas.RenderTransform = new TranslateTransform(newOffsetX, newOffsetY);
                scrollStartPoint = currentPoint;
            }
            else if (isDrawing && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(drawingCanvas);
                DrawShapesBetweenPoints(lastPosition, currentPosition);
                lastPosition = currentPosition;
            }
        }

        #endregion
        #region Wheel
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (InputControl())
            {
                shapeSize += e.Delta > 0 ? 5 : -5;
                if (shapeSize < MinShapeSize) shapeSize = MinShapeSize;
                if (shapeSize > MaxShapeSize) shapeSize = MaxShapeSize;
                UpdateShapeInfo();
            }
            else
            {
                zoomScale += e.Delta > 0 ? 0.1 : -0.1;
                if (zoomScale < MinZoomScale) zoomScale = MinZoomScale;
                if (zoomScale > MaxZoomScale) zoomScale = MaxZoomScale;
                drawingCanvas.LayoutTransform = new ScaleTransform(zoomScale, zoomScale);
            }
        }
        #endregion
        #region Window Events
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            isDrawing = false;
            isScrolling = false;
            Cursor = Cursors.Arrow;
        }
        #endregion
        #endregion

        #region Draw
        private void SelectDrawingElement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectDrawingElement.SelectedIndex == 0)
            {
                currentTool = ToolType.Brush;
            }
            else if (selectDrawingElement.SelectedIndex == 1)
            {
                currentTool = ToolType.Eraser;
            }
        }

        private void DrawShape(Point position)
        {
            if (position.X < 0 || position.X > drawingCanvas.Width || position.Y < 0 || position.Y > drawingCanvas.Height)
            {
                return;
            }

            Shape shape;
            if (currentTool == ToolType.Eraser || InputControl())
            {
                shape = new Rectangle { Width = shapeSize, Height = shapeSize, Fill = Brushes.White };
            }
            else
            {
                shape = new Rectangle { Width = shapeSize, Height = shapeSize, Fill = new SolidColorBrush(selectedColor) };
            }

            Canvas.SetLeft(shape, position.X - shapeSize / 2);
            Canvas.SetTop(shape, position.Y - shapeSize / 2);
            drawingCanvas.Children.Add(shape);
        }

        private void DrawShapesBetweenPoints(Point start, Point end)
        {
            double distance = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
            double step = shapeSize / 2;

            for (double i = 0; i < distance; i += step)
            {
                double t = i / distance;
                double x = (1 - t) * start.X + t * end.X;
                double y = (1 - t) * start.Y + t * end.Y;
                DrawShape(new Point(x, y));
            }
        }
        #endregion


        #region UI
        private void UpdateShapeInfo()
        {
            shapeInfoLabel.Content = $"Size: {shapeSize}";
        }
        #endregion
        #region Saves
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем временный холст для рендеринга
            Canvas renderCanvas = new Canvas
            {
                Width = drawingCanvas.Width,
                Height = drawingCanvas.Height,
                Background = Brushes.White
            };

            // Копируем все элементы из drawingCanvas в renderCanvas
            foreach (UIElement element in drawingCanvas.Children)
            {
                if (element is Shape shape)
                {
                    Shape newShape = null;
                    if (shape is Rectangle ellipse)
                    {
                        newShape = new Rectangle
                        {
                            Width = ellipse.Width,
                            Height = ellipse.Height,
                            Fill = ellipse.Fill,
                            Stroke = ellipse.Stroke,
                            StrokeThickness = ellipse.StrokeThickness
                        };
                    }
                    // Добавьте другие типы фигур, если необходимо

                    if (newShape != null)
                    {
                        Canvas.SetLeft(newShape, Canvas.GetLeft(shape));
                        Canvas.SetTop(newShape, Canvas.GetTop(shape));
                        renderCanvas.Children.Add(newShape);
                    }
                }
            }

            // Рендерим временный холст
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)renderCanvas.Width, (int)renderCanvas.Height, 96d, 96d, PixelFormats.Pbgra32);
            renderCanvas.Measure(new Size((int)renderCanvas.Width, (int)renderCanvas.Height));
            renderCanvas.Arrange(new Rect(new Size((int)renderCanvas.Width, (int)renderCanvas.Height)));
            renderBitmap.Render(renderCanvas);

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var fileStream = System.IO.File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        #endregion
        #region ChangeColor
        private void SelectedColorDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var drawingColor = colorDialog.Color;
                selectedColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                selectedColorDisplay.Fill = new SolidColorBrush(selectedColor);
            }
        }
        #endregion
    }
}