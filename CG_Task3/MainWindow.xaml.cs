using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CG_Task3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // statics
        static readonly System.Drawing.Color selectColor = System.Drawing.Color.CornflowerBlue;

        // flags
        bool currentlyDrawing = false;
        bool isDrawingLine = false;
        bool isDrawingCircle = false;
        bool isDrawingPolygon = false;
        bool isDrawingRectangle = false;
        bool isDrawingTask = false;
        bool isAntialiasingOn = false;

        // buffers
        System.Drawing.Point? pointBuffer = null;
        List<System.Drawing.Point>? multiPointBuffer = null;
        I2DPrimitive? lastSelected = null;
        List<Thumb>? currentSelectionThumbs = null;
        List<Thumb>? currentDrawingThumbs = null;

        // collections
        List<I2DPrimitive> drawnObjects;

        // canvas
        Bitmap drawingCanvas;
        //StylusPointCollection? drawnPixels = null;
        //private List<DDALine> lines = new();

        public MainWindow()
        {
            InitializeComponent();

            // Enabling/disabling buttons
            TaskBt.IsEnabled = false;
            DeleteBt.IsEnabled = false;


            drawnObjects = new List<I2DPrimitive>();
            ObjectsListBox.ItemsSource = drawnObjects;

            // Init drawing canvas
            ResetCanvas();
            Debug.Assert(drawingCanvas != null);
        }

        private void LineBt_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = !isDrawingLine;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingRectangle = false;
            isDrawingTask = false;
            StatusMsgLabel.Content = $"Line drawing = {isDrawingLine}";
            if (isDrawingLine)
            {
                LineBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else
            {
                LineBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }

            DeselectOtherDrawingButtons(LineBt);
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "JSON Files|*.json";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo jsonFileInfo = new FileInfo(dialog.FileName);
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                JArray json = JArray.Parse(File.ReadAllText(jsonFileInfo.FullName));
                IList<JToken> objects = json.Children().ToList();

                foreach(JToken obj in objects )
                {
                    if(obj.First.First.ToString() == "CG_Task3.DDALine, CG_Task3")
                    {
                        DDALine newLine = obj.ToObject<DDALine>();
                        Debug.Assert(null != newLine);
                        drawnObjects.Add(newLine);
                    }
                    else if(obj.First.First.ToString() == "CG_Task3.MidPointCircle, CG_Task3")
                    {
                        MidPointCircle newCircle = obj.ToObject<MidPointCircle>();
                        Debug.Assert(null != newCircle);
                        drawnObjects.Add(newCircle);

                    }
                    else if(obj.First.First.ToString() == "CG_Task3.Polygon, CG_Task3")
                    {
                        Polygon newPolygon = obj.ToObject<Polygon>();
                        Debug.Assert(null != newPolygon);
                        drawnObjects.Add(newPolygon);
                    }
                }

                ResetCanvas();
                RedrawAllObjects();
                ObjectsListBox.Items.Refresh();
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new())
            {
                saveFileDialog.Filter = "JSON Files|*.json";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                    string json = JsonConvert.SerializeObject(drawnObjects, Formatting.Indented, settings);
                    //var options = new JsonSerializerOptions { WriteIndented = true,  };
                    //string json = JsonSerializer.Serialize(drawnObjects, options);
                    File.WriteAllText(saveFileDialog.FileName, json);
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pointBuffer = new System.Drawing.Point((int)e.GetPosition(Canvas).X, (int)e.GetPosition(Canvas).Y);
            if (isDrawingLine)
            {
                currentlyDrawing = true;
                StatusMsgLabel.Content = $"Button down recorded, coords = ({pointBuffer.Value.X},{pointBuffer.Value.Y})";
            }
            else if (isDrawingTask)
            {
                currentlyDrawing = true;
                StatusMsgLabel.Content = $"Button down recorded, coords = ({pointBuffer.Value.X},{pointBuffer.Value.Y})";
            }
            else if (isDrawingCircle)
            {
                currentlyDrawing = true;
                StatusMsgLabel.Content = $"Button down recorded, coords = ({pointBuffer.Value.X},{pointBuffer.Value.Y})";
            }
            else if (isDrawingPolygon)
            {
                currentlyDrawing = true;
                StatusMsgLabel.Content = $"Button down recorded, coords = ({pointBuffer.Value.X},{pointBuffer.Value.Y})";
            }
            else if (isDrawingRectangle)
            {
                currentlyDrawing = true;
                StatusMsgLabel.Content = $"Button down recorded, coords = ({pointBuffer.Value.X},{pointBuffer.Value.Y})";
            }
            else
                StatusMsgLabel.Content = $"Button down recorded but no drawing option selected";
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentlyDrawing)
            {
                var newColor = colorPicker.SelectedColor.ToSystemDrawingColor();
                if (null == newColor)
                    newColor = System.Drawing.Color.Black;

                if (isDrawingLine)
                {
                    currentlyDrawing = false;
                    System.Drawing.Point endPoint = new ((int)e.GetPosition(Canvas).X, (int)e.GetPosition(Canvas).Y);
                    StatusMsgLabel.Content = $"Button up recorded, coords = ({endPoint.X},{endPoint.Y})";

                    Debug.Assert(pointBuffer.HasValue);
                    DDALine newLine = new(pointBuffer.Value, endPoint, newColor.Value, Int32.Parse((string)((ComboBoxItem)ThicknessComboBox.SelectedValue).Content));
                    drawnObjects.Add(newLine);
                    PutObjectOnCanvas(newLine, newLine.Color);
                    ObjectsListBox.UpdateLayout();
                    ObjectsListBox.Items.Refresh();
                }
                else if (isDrawingCircle)
                {
                    currentlyDrawing = false;
                    System.Drawing.Point radiusPoint = new ((int)e.GetPosition(Canvas).X, (int)e.GetPosition(Canvas).Y);
                    StatusMsgLabel.Content = $"Button up recorded, coords = ({radiusPoint.X},{radiusPoint.Y})";

                    Debug.Assert(pointBuffer.HasValue);
                    int radius = (int)Math.Round(Math.Sqrt(Math.Pow(pointBuffer.Value.X - radiusPoint.X, 2) + Math.Pow(pointBuffer.Value.Y - radiusPoint.Y, 2)));
                    MidPointCircle newCircle = new(pointBuffer.Value, radius, newColor.Value);;
                    //if(newCircle.Pixels.Where(pixel => (pixel.X >= 0 && pixel.X < 800 && pixel.Y >= 0 && pixel.Y < 600)) {

                    //}
                    drawnObjects.Add(newCircle);
                    PutObjectOnCanvas(newCircle, newCircle.Color);
                    ObjectsListBox.UpdateLayout();
                    ObjectsListBox.Items.Refresh();
                }
                else if (isDrawingTask)
                {
                    /*
                    currentlyDrawing = false;
                    StylusPoint endPoint = new(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
                    endPoint = new StylusPoint(Math.Round(endPoint.X), Math.Round(endPoint.Y));
                    StatusMsgLabel.Content = $"Button up recorded, coords = ({endPoint.X},{endPoint.Y})";

                    Debug.Assert(pointBuffer.HasValue);
                    //TaskShape newTaskShape = new(pointBuffer.Value, endPoint);
                    */
                }
                else if (isDrawingPolygon)
                {
                    if(null == multiPointBuffer)
                    {
                        multiPointBuffer = new();
                        currentDrawingThumbs = new();
                    }

                    System.Drawing.Point point = new ((int)e.GetPosition(Canvas).X, (int)e.GetPosition(Canvas).Y);
                    StatusMsgLabel.Content = $"Button up recorded, coords = ({point.X},{point.Y})";

                    // polygon closing condition
                    if (multiPointBuffer.Count >= 2 && Math.Abs(point.X - multiPointBuffer.ElementAt(0).X) < 20 && Math.Abs(point.Y - multiPointBuffer.ElementAt(0).Y) < 20)
                    {
                        currentlyDrawing = false;
                        Polygon newPolygon = new(multiPointBuffer, newColor.Value);
                        drawnObjects.Add(newPolygon);
                        PutObjectOnCanvas(newPolygon, newPolygon.Color);
                        ObjectsListBox.UpdateLayout();
                        ObjectsListBox.Items.Refresh();
                        multiPointBuffer.Clear();
                        if (null != currentDrawingThumbs)
                        {
                            foreach (Thumb thumbChild in currentDrawingThumbs)
                                CanvasFrame.Children.Remove(thumbChild);

                            currentDrawingThumbs.Clear();
                        }
                    }
                    else
                    {
                        currentDrawingThumbs.Add(new Thumb());
                        Thumb current = currentDrawingThumbs.Last();
                        CanvasFrame.Children.Add(current);
                        var thumbTemplate = (ControlTemplate)Application.Current.MainWindow.Resources["EditHandleTemplate"];
                        current.Template = thumbTemplate;
                        System.Windows.Controls.Canvas.SetLeft(current, point.X - 5); // 5 is 1/2 of Width in EditHandleTemplate
                        System.Windows.Controls.Canvas.SetTop(current, point.Y - 5); // 5 is 1/2 of Height in EditHandleTemplate
                        multiPointBuffer.Add(point); // only if not closing
                        StatusMsgLabel.Content = $"MultiPointBuffer contains {multiPointBuffer}";
                    }
                }
                else if (isDrawingRectangle)
                {
                    currentlyDrawing = false;
                    System.Drawing.Point endPoint = new ((int)e.GetPosition(Canvas).X, (int)e.GetPosition(Canvas).Y);
                    StatusMsgLabel.Content = $"Button up recorded, coords = ({endPoint.X},{endPoint.Y})";

                    Debug.Assert(pointBuffer.HasValue);
                    Rectangle newRectangle = new(pointBuffer.Value, endPoint, newColor.Value);
                    drawnObjects.Add(newRectangle);
                    PutObjectOnCanvas(newRectangle, newRectangle.Color);
                    ObjectsListBox.UpdateLayout();
                    ObjectsListBox.Items.Refresh();
                }
            }
            else
                StatusMsgLabel.Content = $"Button up recorded but no drawing option selected";
        }

        private void ObjectsListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            // Unselect previously selected items
            if (null != lastSelected)
                RedrawAllObjects();         // TODO: Only partial redraw would improve performance

            // Erase previous edit handles
            if (null != currentSelectionThumbs)
            {
                foreach (Thumb thumbChild in currentSelectionThumbs)
                    CanvasFrame.Children.Remove(thumbChild);

                currentSelectionThumbs.Clear();
            }

            if (null != ObjectsListBox.SelectedItem)
            {
                // Selecting the new stroke
                I2DPrimitive? selectedObject = (I2DPrimitive)ObjectsListBox.SelectedItem;
                StatusMsgLabel.Content = $"Object selected = {selectedObject}";
                PutObjectOnCanvas(selectedObject, selectColor);


                currentSelectionThumbs = new List<Thumb>();
                foreach (System.Drawing.Point handlePoint in selectedObject.HandlePoints)
                {
                    currentSelectionThumbs.Add(new Thumb());
                    Thumb current = currentSelectionThumbs.Last();
                    current.Tag = selectedObject.HandlePoints.IndexOf(handlePoint);
                    CanvasFrame.Children.Add(current);
                    var thumbTemplate = (ControlTemplate)Application.Current.MainWindow.Resources["EditHandleTemplate"];
                    current.Template = thumbTemplate;
                    current.DragDelta += OnDragDelta;
                    System.Windows.Controls.Canvas.SetLeft(current, handlePoint.X - 5); // 5 is 1/2 of Width in EditHandleTemplate
                    System.Windows.Controls.Canvas.SetTop(current, handlePoint.Y - 5); // 5 is 1/2 of Height in EditHandleTemplate
                }

                lastSelected = selectedObject;
                DeleteBt.IsEnabled = true;
            }
            else
            {
                DeleteBt.IsEnabled = false;
                lastSelected = null;
            }
        }

        private void TaskBt_Click(object sender, RoutedEventArgs e)
        {
            isDrawingTask = !isDrawingTask;
            StatusMsgLabel.Content = $"Task drawing = {isDrawingTask}";
            if (isDrawingTask)
            {
                TaskBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else
            {
                TaskBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }

        }

        private void DeleteBt_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(lastSelected != null);
            drawnObjects.Remove(lastSelected);
            lastSelected = null;
            ObjectsListBox.Items.Refresh();
            DeleteBt.IsEnabled = false;
            if (null != currentSelectionThumbs)
            {
                foreach (Thumb thumbChild in currentSelectionThumbs)
                    CanvasFrame.Children.Remove(thumbChild);

                currentSelectionThumbs.Clear();
            }
            RedrawAllObjects();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb handle = (Thumb)sender;

            double newX = Math.Round(System.Windows.Controls.Canvas.GetLeft(handle) + e.HorizontalChange);
            double newY = Math.Round(System.Windows.Controls.Canvas.GetTop(handle) + e.VerticalChange);

            // Move the handle
            System.Windows.Controls.Canvas.SetLeft(handle, newX - 5);
            System.Windows.Controls.Canvas.SetTop(handle, newY - 5);

            // Move the object's handle point
            int handlePointIndex = (int)handle.Tag;
            Debug.Assert(null != lastSelected);
            int x = lastSelected.HandlePoints.ElementAt(handlePointIndex).X;
            int y = lastSelected.HandlePoints.ElementAt(handlePointIndex).Y;
            StatusMsgLabel.Content = $"Modify point ({x}, {y}) of the object {lastSelected}";
            System.Drawing.Point newPoint = new((int)newX, (int)newY);
            var handlePoints = lastSelected.HandlePoints;
            handlePoints.RemoveAt(handlePointIndex);
            handlePoints.Insert(handlePointIndex, newPoint);
            lastSelected.HandlePoints = handlePoints;

            // Update the drawn object
            RedrawAllObjects();     // TODO: partial redraw for performance
            PutObjectOnCanvas(lastSelected, selectColor);
            ObjectsListBox.Items.Refresh();
        }

        private void PutObjectOnCanvas(I2DPrimitive drawnObject, System.Drawing.Color color)
        {
            System.Drawing.Rectangle encompassingRect = GetEncompassingRectangle(drawnObject);
            BitmapData canvasPartData = drawingCanvas.LockBits(encompassingRect, ImageLockMode.ReadWrite, drawingCanvas.PixelFormat);
            int bytes = canvasPartData.Stride * encompassingRect.Height;
            byte[] argbValues = new byte[bytes];
            IntPtr ptr = canvasPartData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, bytes);


            if(isAntialiasingOn && drawnObject.GetType() == typeof(DDALine))
            {

            }
            else
                foreach (System.Drawing.Point pixel in drawnObject.Pixels)
                {
                    argbValues[(pixel.Y - encompassingRect.Y) * canvasPartData.Stride + (pixel.X - encompassingRect.X) * 4 + (int)ColorChannels.Blue] = color.B;
                    argbValues[(pixel.Y - encompassingRect.Y) * canvasPartData.Stride + (pixel.X - encompassingRect.X) * 4 + (int)ColorChannels.Green] = color.G;
                    argbValues[(pixel.Y - encompassingRect.Y) * canvasPartData.Stride + (pixel.X - encompassingRect.X) * 4 + (int)ColorChannels.Red] = color.R;
                }

            System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, bytes);
            drawingCanvas.UnlockBits(canvasPartData);
            RefreshCanvasDisplay();
        }

        private void RefreshCanvasDisplay()
        {
            using (MemoryStream ms = new())
            {
                drawingCanvas.Save(ms, ImageFormat.Png);
                BitmapImage bitmap = new();
                ms.Position = 0;
                bitmap.BeginInit();
                //bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;   // is there a way to actually make this bitmap rgba? this does not work
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                Canvas.Source = bitmap;
            }
        }

        private static System.Drawing.Rectangle GetEncompassingRectangle(I2DPrimitive figure)
        {
            int x = figure.Pixels.Min(pixel => pixel.X);
            int y = figure.Pixels.Min(pixel => pixel.Y);
            int width = figure.Pixels.Max(pixel => pixel.X) -  x + 1;
            int height = figure.Pixels.Max(pixel => pixel.Y) - y + 1;

            Debug.Assert(x >= 0);
            Debug.Assert(y >= 0);

            return new System.Drawing.Rectangle(x, y, width, height);
        }

        private void ResetCanvas()
        {
            drawingCanvas = new((int)CanvasFrame.Width, (int)CanvasFrame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Rectangle rect = new(0, 0, drawingCanvas.Width, drawingCanvas.Height);
            BitmapData canvasData = drawingCanvas.LockBits(rect, ImageLockMode.WriteOnly, drawingCanvas.PixelFormat);
            int bytes = Math.Abs(canvasData.Stride) * drawingCanvas.Height;
            byte[] argbValues = new byte[bytes];
            Array.Fill(argbValues, (byte)255);
            IntPtr ptr = canvasData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, bytes);
            drawingCanvas.UnlockBits(canvasData);
            RefreshCanvasDisplay();
        }

        private void RedrawAllObjects()
        {
            ResetCanvas();
            foreach (I2DPrimitive drawnObject in drawnObjects)
                PutObjectOnCanvas(drawnObject, drawnObject.Color);

            RefreshCanvasDisplay();
        }

        private void CircleBt_Click(object sender, RoutedEventArgs e)
        {
            isDrawingCircle = !isDrawingCircle;
            isDrawingLine = false;
            isDrawingTask = false;
            isDrawingRectangle = false;
            isDrawingPolygon = false;
            StatusMsgLabel.Content = $"Circle drawing = {isDrawingCircle}";
            if (isDrawingCircle)
            {
                CircleBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else
            {
                CircleBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }

            DeselectOtherDrawingButtons(CircleBt);
        }
        
        private void DeselectOtherDrawingButtons(Button clickedButton)
        {
            foreach (UIElement element in LeftToolbar.Children)
            {
                Button button = element as Button;
                if(null != button)
                {
                    if(button.Name != clickedButton.Name)
                        button.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
                }
            }
        }

        private void PolygonBt_Click(object sender, RoutedEventArgs e)
        {
            isDrawingPolygon = !isDrawingPolygon;
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingTask = false;
            StatusMsgLabel.Content = $"Polygon drawing = {isDrawingPolygon}";
            if (isDrawingPolygon)
            {
                PolygonBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else
            {
                PolygonBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }

            DeselectOtherDrawingButtons(PolygonBt);
        }

        private void ResetCanvasBt_Click(object sender, RoutedEventArgs e)
        {
            ResetCanvas();

            System.Drawing.Point? pointBuffer = null;
            List<System.Drawing.Point>? multiPointBuffer = null;
            I2DPrimitive? lastSelected = null;
            List<Thumb>? currentSelectionThumbs = null;
            List<Thumb>? currentDrawingThumbs = null;
            drawnObjects.Clear();
            ObjectsListBox.Items.Refresh();

            DeleteBt.IsEnabled = false;
            Debug.Assert(drawnObjects.Count == 0);
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if(null != lastSelected)
            {
                Debug.Assert(null != e.NewValue);
                var newColor = e.NewValue.ToSystemDrawingColor();
                if (null == newColor)
                    newColor = System.Drawing.Color.Black;
                lastSelected.Color = newColor.Value;
                PutObjectOnCanvas(lastSelected, lastSelected.Color);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(null != lastSelected && lastSelected.GetType() == typeof(DDALine))
            {
                ComboBox box = (ComboBox)sender;
                Debug.Assert(null != box);
                ((DDALine)lastSelected).BrushThickness = Int32.Parse((string)((ComboBoxItem)box.SelectedValue).Content);
                RedrawAllObjects();
                PutObjectOnCanvas(lastSelected, selectColor);
            }
        }

        private void AntialiasingBt_Click(object sender, RoutedEventArgs e)
        {
            isAntialiasingOn = !isAntialiasingOn;
            RedrawAllObjects();
        }

        void ThickAntialiasedLine(int x1, int y1, int x2, int y2, float thickness)
        {
            //initial values in Bresenham;s algorithm
            int dx = x2 - x1, dy = y2 - y1;
            int dE = 2 * dy, dNE = 2 * (dy - dx);
            int d = 2 * dy - dx;
            int two_v_dx = 0; //numerator, v=0 for the first pixel
            float invDenom = (float)(1 / (2 * Math.Sqrt(dx * dx + dy * dy))); //inverted denominator
            float two_dx_invDenom = 2 * dx * invDenom; //precomputed constant
            int x = x1, y = y1;
            int i;
            IntensifyPixel(x, y, thickness, 0);
            for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom) == 1; ++i);
            for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom) == 1; ++i);
            while (x < x2)
            {
                ++x;
                if (d < 0) // move to E
                {
                    two_v_dx = d + dx;
                    d += dE;
                }
                else // move to NE
                {
                    two_v_dx = d - dx;
                    d += dNE;
                    ++y;
                }
                // Now set the chosen pixel and its neighbors
                IntensifyPixel(x, y, thickness, two_v_dx * invDenom);
                for (i = 1; IntensifyPixel(x, y + i, thickness, i * two_dx_invDenom - two_v_dx * invDenom) == 1; ++i) ;
                for (i = 1; IntensifyPixel(x, y - i, thickness, i * two_dx_invDenom + two_v_dx * invDenom) == 1; ++i) ;
            }
        }

        int IntensifyPixel(int x, int y, float thickness, float distance)
        {
            float r = 0.5f;
            //float cov = coverage(thickness, distance, r);
            //if (cov > 0)
            //putPixel(x, y, lerp(BKG_COLOR, LINE_COLOR, cov));
            //return cov;
            return -999;
        }

        private void RectangleBt_Click(object sender, RoutedEventArgs e)
        {
            isDrawingRectangle = !isDrawingRectangle;
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingTask = false;
            StatusMsgLabel.Content = $"Rectangle drawing = {isDrawingRectangle}";
            if (isDrawingRectangle)
            {
                RectangleBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else
            {
                RectangleBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }

            DeselectOtherDrawingButtons(RectangleBt);
        }
    }
}
