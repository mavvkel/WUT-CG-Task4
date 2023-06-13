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
using System.Threading;
using System.Windows.Threading;

namespace CG_Task3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // development DEBUG
        bool isDebugging = true;

        // statics
        static readonly System.Drawing.Color selectColor = System.Drawing.Color.CornflowerBlue;
        static readonly System.Drawing.Color clippingColor = System.Drawing.Color.Orange;
        static readonly System.Drawing.Color clippingResultColor = System.Drawing.Color.Red;

        // flags
        bool currentlyDrawing = false;
        bool isDrawingLine = false;
        bool isDrawingCircle = false;
        bool isDrawingPolygon = false;
        bool isDrawingRectangle = false;
        bool isDrawingTask = false;
        bool isAntialiasingOn = false;
        bool isClipping = false;
        bool isBucketFilling = false;

        // buffers
        System.Drawing.Point? pointBuffer = null;
        List<System.Drawing.Point>? multiPointBuffer = null;
        I2DPrimitive? lastSelected = null;
        List<Thumb>? currentSelectionThumbs = null;
        List<Thumb>? currentDrawingThumbs = null;
        List<Thumb>? clippingThumbs = null;
        Polygon? clippingPolygon = null;
        I2DPrimitive? clippedObject = null;

        // collections
        List<I2DPrimitive> drawnObjects;

        // canvas
        Bitmap drawingCanvas;

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


        #region UIButtonsEventHandlers

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
                    else if(obj.First.First.ToString() == "CG_Task3.Rectangle, CG_Task3")
                    {
                        Rectangle newRectangle = obj.ToObject<Rectangle>();
                        Debug.Assert(null != newRectangle);
                        drawnObjects.Add(newRectangle);
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

        private void PolygonBt_Click(object sender, RoutedEventArgs e)
        {
            isDrawingPolygon = !isDrawingPolygon;
            isDrawingLine = false;
            isDrawingRectangle = false;
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

        private void AntialiasingBt_Click(object sender, RoutedEventArgs e)
        {
            isAntialiasingOn = !isAntialiasingOn;
            RedrawAllObjects();
        }

        private void ClippingBt_Click(object sender, RoutedEventArgs e)
        {
            isClipping = !isClipping;
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingRectangle = false;
            isDrawingTask = false;

            DeselectOtherDrawingButtons(PolygonBt);
            PolygonBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];

            if (isClipping)
            {
                ClippingBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
                if(null != lastSelected)
                {
                    clippedObject = lastSelected;
                    StatusMsgLabel.Content = $"Clipping mode = {isClipping}.";
                }
                else
                {
                    StatusMsgLabel.Content = $"Clipping mode = {isClipping}. Select the clipped object.";
                }
            }
            else
            {
                StatusMsgLabel.Content = $"Clipping mode = {isClipping}.";
                clippingPolygon = null;
                currentSelectionThumbs = null;
                clippedObject = null;
                ClippingBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }

        }

        private void BucketFillBt_Click(object sender, RoutedEventArgs e)
        {
            isBucketFilling = !isBucketFilling;
            isClipping = false;
            isDrawingLine = false;
            isDrawingCircle = false;
            isDrawingPolygon = false;
            isDrawingRectangle = false;
            isDrawingTask = false;

            DeselectOtherDrawingButtons(PolygonBt);
            if (isBucketFilling)
            {
                BucketFillBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else
            {
                BucketFillBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }
            StatusMsgLabel.Content = $"Bucket filling = {isBucketFilling}.";
        }

        private void FillBt_Click(object sender, RoutedEventArgs e)
        {
            if(null != lastSelected)
            {
                List<(int ymax, double x_min, double grad)> activeEdgeTable = new();
                List<System.Drawing.Point> vertices = lastSelected.HandlePoints; 
                var sorted = lastSelected.HandlePoints
                    .Select((pt, i) => new KeyValuePair<System.Drawing.Point, int>(pt, i))
                    .OrderBy(pt => pt.Key.Y)
                    .ToList();
                List<int> indices = sorted.Select(pair => pair.Value).ToList();
                Blob toBeFilled = new(new List<System.Drawing.Point>());
                int n = vertices.Count;

                int k = 0;
                int i = indices[k];
                int y = vertices[indices[0]].Y;
                int ymin = y;
                int ymax = vertices[indices[n - 1]].Y;
                int DEBUGtestcounter = 0;
                while(y < ymax) // && DEBUGtestcounter < 100)
                {
                    DEBUGtestcounter++;

                    while (vertices[i].Y == y)
                    {
                        if (vertices[mod(i - 1, n)].Y > vertices[i].Y)
                            activeEdgeTable.Add(GetAETEntry(vertices[i], vertices[mod(i - 1, n)]));
                        if (vertices[mod(i + 1, n)].Y > vertices[i].Y)
                            activeEdgeTable.Add(GetAETEntry(vertices[i], vertices[mod(i + 1, n)]));
                        ++k;
                        i = indices[k];
                    }
                    activeEdgeTable = activeEdgeTable.OrderBy(entry => entry.x_min).ToList();
                    // fill pixels between the pairs of intersections
                    for (int j = 0; j < activeEdgeTable.Count; j += 2)
                    {
                        ///Debug.Assert(3 > activeEdgeTable.Count);
                        int x_current = (int)Math.Round(activeEdgeTable[j].x_min);
                        int x_last = (int)Math.Round(activeEdgeTable[j + 1].x_min);
                        while (x_current <= x_last)
                        {
                            toBeFilled.Pixels.Add(new(x_current, y));
                            x_current++;
                        }
                    }

                    ++y;
                    activeEdgeTable = activeEdgeTable
                        .Where(entry => entry.ymax != y)
                        .ToList();
                    activeEdgeTable = activeEdgeTable
                        .Select(entry => { entry.x_min = entry.x_min + entry.grad; return entry; })
                        .ToList();
                }
                var fillColor = colorPicker.SelectedColor.ToSystemDrawingColor();
                if (null == fillColor)
                    fillColor = System.Drawing.Color.Black;
                PutObjectOnCanvas(toBeFilled, fillColor.Value);
            }
        }

        #endregion


        #region UICanvasEventHandlers

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
            if (currentlyDrawing || isClipping)
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
                else if (isDrawingPolygon || (isClipping && null == clippingPolygon && null != clippedObject)) // either drawing a polygon or drawing a clipping polygon
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
                        if (isClipping)
                            newColor = clippingColor;
                        Polygon newPolygon = new(multiPointBuffer, newColor.Value);
                        if(!isClipping)
                        {
                            drawnObjects.Add(newPolygon);
                            ObjectsListBox.UpdateLayout();
                            ObjectsListBox.Items.Refresh();
                            if (null != currentDrawingThumbs)
                            {
                                foreach (Thumb thumbChild in currentDrawingThumbs)
                                    CanvasFrame.Children.Remove(thumbChild);

                                currentDrawingThumbs.Clear();
                            }
                        }
                        else
                        {
                            clippingPolygon = newPolygon;
                            clippingThumbs = currentDrawingThumbs;
                            currentDrawingThumbs = null;

                            // ASSUME A POLYGON IS BEING CLIPPED
                            Blob clippingResult = GetClippingResult((Polygon)clippedObject, clippingPolygon);
                            PutObjectOnCanvas(clippingResult, clippingResultColor);
                        }
                        PutObjectOnCanvas(newPolygon, newPolygon.Color);
                        multiPointBuffer.Clear();
                    }
                    else // case when polygon is not yet being closed
                    {
                        multiPointBuffer.Add(point); 

                        currentDrawingThumbs.Add(new Thumb());
                        Thumb current = currentDrawingThumbs.Last();
                        CanvasFrame.Children.Add(current);
                        ControlTemplate thumbTemplate;
                        if(!isClipping)
                            thumbTemplate = (ControlTemplate)Application.Current.MainWindow.Resources["EditHandleTemplate"];
                        else
                            thumbTemplate = (ControlTemplate)Application.Current.MainWindow.Resources["ClipEditHandleTemplate"];

                        current.Template = thumbTemplate;
                        current.Tag = multiPointBuffer.IndexOf(point);
                        current.DragDelta += OnDragDelta;
                        System.Windows.Controls.Canvas.SetLeft(current, point.X - 5); // 5 is 1/2 of Width in EditHandleTemplate
                        System.Windows.Controls.Canvas.SetTop(current, point.Y - 5); // 5 is 1/2 of Height in EditHandleTemplate
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
            else if(isBucketFilling)
            {
                System.Drawing.Point clickedPoint = new((int)e.GetPosition(Canvas).X, (int)e.GetPosition(Canvas).Y);
                System.Drawing.Color clickedPointColor = GetPixelColor(clickedPoint.X, clickedPoint.Y);
                //int R = clickedPointColor.R;

                List<System.Drawing.Point> pointsToBeFilled = new();
                Queue<System.Drawing.Point> pointsToBeVisited = new();
                List<System.Drawing.Point> pointsVisited = new();
                System.Drawing.Point currentPoint = clickedPoint;

                do
                {
                    // left
                    if (currentPoint.X - 1 >= 0 && GetPixelColor(currentPoint.X - 1, currentPoint.Y) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X - 1, currentPoint.Y))
                         && !pointsToBeVisited.Contains(new(currentPoint.X - 1, currentPoint.Y)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X - 1, currentPoint.Y));

                    // top left
                    if (currentPoint.X - 1 >= 0 && currentPoint.Y - 1 >= 0 && GetPixelColor(currentPoint.X - 1, currentPoint.Y - 1) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X - 1, currentPoint.Y - 1))
                         && !pointsToBeVisited.Contains(new(currentPoint.X - 1, currentPoint.Y - 1)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X - 1, currentPoint.Y - 1));

                    // top 
                    if (currentPoint.Y - 1 >= 0 && GetPixelColor(currentPoint.X, currentPoint.Y - 1) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X, currentPoint.Y - 1))
                         && !pointsToBeVisited.Contains(new(currentPoint.X, currentPoint.Y - 1)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X, currentPoint.Y - 1));

                    // top right 
                    if (currentPoint.Y - 1 >= 0 && currentPoint.X + 1 < drawingCanvas.Width && GetPixelColor(currentPoint.X + 1, currentPoint.Y - 1) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X + 1, currentPoint.Y - 1))
                         && !pointsToBeVisited.Contains(new(currentPoint.X + 1, currentPoint.Y - 1)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X + 1, currentPoint.Y - 1));

                    // right 
                    if (currentPoint.X + 1 < drawingCanvas.Width && GetPixelColor(currentPoint.X + 1, currentPoint.Y) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X + 1, currentPoint.Y))
                         && !pointsToBeVisited.Contains(new(currentPoint.X + 1, currentPoint.Y)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X + 1, currentPoint.Y));

                    // bottom right 
                    if (currentPoint.X + 1 < drawingCanvas.Width && currentPoint.Y + 1 < drawingCanvas.Height && GetPixelColor(currentPoint.X + 1, currentPoint.Y + 1) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X + 1, currentPoint.Y + 1))
                         && !pointsToBeVisited.Contains(new(currentPoint.X + 1, currentPoint.Y + 1)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X + 1, currentPoint.Y + 1));

                    // bottom 
                    if (currentPoint.Y + 1 < drawingCanvas.Height && GetPixelColor(currentPoint.X + 1, currentPoint.Y + 1) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X, currentPoint.Y + 1))
                         && !pointsToBeVisited.Contains(new(currentPoint.X, currentPoint.Y + 1)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X, currentPoint.Y + 1));

                    // bottom left
                    if (currentPoint.X - 1 >= 0 && currentPoint.Y + 1 < drawingCanvas.Height && GetPixelColor(currentPoint.X + 1, currentPoint.Y + 1) == clickedPointColor && !pointsVisited.Contains(new(currentPoint.X - 1, currentPoint.Y + 1))
                         && !pointsToBeVisited.Contains(new(currentPoint.X - 1, currentPoint.Y + 1)))
                        pointsToBeVisited.Enqueue(new(currentPoint.X - 1, currentPoint.Y + 1));


                    Debug.Assert(!pointsVisited.Contains(currentPoint));
                    pointsVisited.Add(currentPoint);

                    StatusMsgLabel.Content = $"{currentPoint.ToString}";
                    pointsToBeFilled.Add(currentPoint);
                    currentPoint = pointsToBeVisited.Dequeue();

                } while (pointsToBeVisited.Count != 0);

                Debug.Assert(0 != pointsToBeFilled.Count);
                Blob blob = new(pointsToBeFilled);
                drawnObjects.Add(blob);
                PutObjectOnCanvas(blob, selectColor);
                RefreshCanvasDisplay();
            }
            else
                StatusMsgLabel.Content = $"Button up recorded but no drawing option selected";
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb handle = (Thumb)sender;

            double newX = Math.Round(System.Windows.Controls.Canvas.GetLeft(handle) + e.HorizontalChange);
            double newY = Math.Round(System.Windows.Controls.Canvas.GetTop(handle) + e.VerticalChange);

            // # Move the object handle point
            int handlePointIndex = (int)handle.Tag;
            Debug.Assert(null != lastSelected || null != clippingPolygon);

            // ## Decide which object to adjust - selected or clipping rectangle
            I2DPrimitive? adjustedObject;
            System.Drawing.Color drawColor;
            if(null != clippingThumbs && clippingThumbs.Contains(handle))      // case when clipping rectangle handle is moved
            {
                adjustedObject = clippingPolygon;
                drawColor = clippingColor;
            }
            else    // case wehen selected object's handle is moved
            {
                adjustedObject = lastSelected;
                drawColor = selectColor;
            }

            List<System.Drawing.Point> handlePoints = adjustedObject.HandlePoints.ConvertAll(point => new System.Drawing.Point(point.X, point.Y));


            // Change other related handle points if necessary
            if(handlePointIndex == (int)Nums.CenterHandleTag)
            {
                for (int i = 0; i < handlePoints.Count; i++)
                {
                    if (i != handlePointIndex)
                    {
                        System.Drawing.Point newPt = new((int)Math.Round(handlePoints[i].X + e.HorizontalChange), (int)Math.Round(handlePoints[i].Y + e.VerticalChange));
                        handlePoints.RemoveAt(i);
                        handlePoints.Insert(i, newPt);
                    }
                }
            }
            else
            {
                // Change the adjusted handle
                int x = adjustedObject.HandlePoints.ElementAt(handlePointIndex).X;
                int y = adjustedObject.HandlePoints.ElementAt(handlePointIndex).Y;
                StatusMsgLabel.Content = $"Modify point ({x}, {y}) of the object {adjustedObject}";
                System.Drawing.Point newPoint = new((int)newX, (int)newY);
                handlePoints.RemoveAt(handlePointIndex);
                handlePoints.Insert(handlePointIndex, newPoint);
            }

            adjustedObject.HandlePoints = handlePoints;

            RepositionObjectHandles(adjustedObject);

            // Update the drawn object
            RedrawAllObjects();     // TODO: partial redraw for performance
            PutObjectOnCanvas(adjustedObject, drawColor);
            if (isClipping)
            {
                PutObjectOnCanvas(clippingPolygon, clippingColor);
                PutObjectOnCanvas(lastSelected, selectColor);
            }
            ObjectsListBox.Items.Refresh();

        }

        #endregion


        #region CanvasHelpers

        private void PutObjectOnCanvas(I2DPrimitive drawnObject, System.Drawing.Color color)
        {
            System.Drawing.Rectangle encompassingRect = GetEncompassingRectangle(drawnObject);
            BitmapData canvasPartData = drawingCanvas.LockBits(encompassingRect, ImageLockMode.ReadWrite, drawingCanvas.PixelFormat);
            int bytes = canvasPartData.Stride * encompassingRect.Height;
            byte[] argbValues = new byte[bytes];
            IntPtr ptr = canvasPartData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, bytes);


            if(isAntialiasingOn && drawnObject.GetType() == typeof(DDALine) && false)
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

        private void RepositionObjectHandles(I2DPrimitive obj)
        {
            Debug.Assert(null != obj);
            Debug.Assert(null != currentSelectionThumbs);
            List<Thumb> currentObjThumbs;
            if (obj == clippingPolygon)
                currentObjThumbs = clippingThumbs;
            else
                currentObjThumbs = currentSelectionThumbs;
            for (int i = 0; i < obj.HandlePoints.Count; ++i)
            {
                System.Windows.Controls.Canvas.SetLeft(currentObjThumbs[i], obj.HandlePoints[i].X - 5); // 5 is 1/2 of Width in EditHandleTemplate
                System.Windows.Controls.Canvas.SetTop(currentObjThumbs[i], obj.HandlePoints[i].Y - 5); // 5 is 1/2 of Height in EditHandleTemplate
            }

            System.Windows.Controls.Canvas.SetLeft(currentObjThumbs.Last(), obj.CenterHandlePoint.X - 5); // 5 is 1/2 of Width in EditHandleTemplate
            System.Windows.Controls.Canvas.SetTop(currentObjThumbs.Last(), obj.CenterHandlePoint.Y - 5); // 5 is 1/2 of Height in EditHandleTemplate

        }

        #endregion


        #region OtherHelpers

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

        private System.Drawing.Color GetPixelColor(int x, int y)
        {
            BitmapData canvasPixelData = drawingCanvas.LockBits(new System.Drawing.Rectangle(x, y, 1, 1), ImageLockMode.ReadOnly, drawingCanvas.PixelFormat);
            int bytes = 4;
            byte[] argbValue = new byte[bytes];
            IntPtr ptr = canvasPixelData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(ptr, argbValue, 0, bytes);

            System.Drawing.Color pixelColor = System.Drawing.Color.FromArgb(255, argbValue[(int)ColorChannels.Red], argbValue[(int)ColorChannels.Green], argbValue[(int)ColorChannels.Blue]);
            drawingCanvas.UnlockBits(canvasPixelData);
            return pixelColor;
        }

        private (int, double, double) GetAETEntry(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            int y_max = Math.Max(p1.Y, p2.Y);
            double x = (y_max == p1.Y) ? p2.X : p1.X; 
            double gradReciprocal = 1f / ((double)(p1.Y - p2.Y) / (double)(p1.X - p2.X));

            if (true == isDebugging)
            {
                // highlight current edge 
                DDALine currentLine = new(p1, p2, System.Drawing.Color.Red);
                PutObjectOnCanvas(currentLine, currentLine.Color);
            }

            return (y_max, x, gradReciprocal);

        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        private static int[] GenerateRandomIntArray(int minValue, int maxValue, int lenght)
        {
            Random rng = new();
            int[] array = Enumerable
                .Repeat(0, lenght)
                .Select(i => rng.Next(minValue, maxValue))
                .ToArray();
            return array;
        }

        private static double Dot(Vector2D v1, Vector2D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        #endregion


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
                    var centerThumbTemplate = (ControlTemplate)Application.Current.MainWindow.Resources["EditHandleTemplate"];
                    current.Template = centerThumbTemplate;
                    current.DragDelta += OnDragDelta;
                    System.Windows.Controls.Canvas.SetLeft(current, handlePoint.X - 5); // 5 is 1/2 of Width in EditHandleTemplate
                    System.Windows.Controls.Canvas.SetTop(current, handlePoint.Y - 5); // 5 is 1/2 of Height in EditHandleTemplate
                }

                // Add center handle thumb
                currentSelectionThumbs.Add(new Thumb());
                Thumb centerThumb = currentSelectionThumbs.Last();
                centerThumb.Tag = Nums.CenterHandleTag;
                CanvasFrame.Children.Add(centerThumb);
                var thumbTemplate = (ControlTemplate)Application.Current.MainWindow.Resources["EditCenterHandleTemplate"];
                centerThumb.Template = thumbTemplate;
                centerThumb.DragDelta += OnDragDelta;
                System.Windows.Controls.Canvas.SetLeft(centerThumb, selectedObject.CenterHandlePoint.X - 5); // 5 is 1/2 of Width in EditHandleTemplate
                System.Windows.Controls.Canvas.SetTop(centerThumb, selectedObject.CenterHandlePoint.Y - 5); // 5 is 1/2 of Height in EditHandleTemplate

                lastSelected = selectedObject;
                DeleteBt.IsEnabled = true;
            }
            else
            {
                DeleteBt.IsEnabled = false;
                lastSelected = null;
            }
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
            if(null != lastSelected)
            {
                ComboBox box = (ComboBox)sender;
                Debug.Assert(null != box);
                lastSelected.BrushThickness = Int32.Parse((string)((ComboBoxItem)box.SelectedValue).Content);
                RedrawAllObjects();
                PutObjectOnCanvas(lastSelected, selectColor);
            }
        }

        private Blob GetClippingResult(Polygon clippedPoly, Polygon clippingPoly)
        {
            Blob clippingResult = new(new List<System.Drawing.Point>());
            List<DDALine> clippingEdges = clippingPoly.Edges;
            int n = clippingEdges.Count;
            bool hasBroken = false;

            // calculate Ni & choose PEi for each clipping line
            List<Vector2D> N = clippingEdges
                .Select(edge => new Vector2D(edge.StartPoint.Y - edge.EndPoint.Y, edge.EndPoint.X - edge.StartPoint.X))
                .Select(vec =>
                {
                    if (vec.Y == 0)
                        return vec;
                    else
                        return new Vector2D(vec.X, vec.Y * (-1));
                }).ToList();
            System.Drawing.Point[] PE = clippingEdges.Select(edge => edge.CenterHandlePoint).ToArray();

            //for (int i = 0; i < clippingEdges.Count; i++)
            //{
            //    N[i] = clippingPoly.HandlePoints[(i + 1) % clippingEdges.Count].X - clippingPoly.HandlePoints[i].X;
            //    N[i]= clippingPoly.HandlePoints[i].Y - clippingPoly.HandlePoints[(i + 1) % clippingEdges.Count].Y;
            //    PE[i] = clippingEdges[i].StartPoint;
            //}

            foreach (var clippedLine in clippedPoly.Edges)
            {
                if(clippedLine.StartPoint == clippedLine.EndPoint)
                {
                    // clip as a point
                }
                else
                {
                    double tE = 0;
                    double tL = 1;

                    //foreach (var clippingLine in clippingEdges)
                    for (int i = 0; i < n; i++)
                    {
                        Vector2D D = new Vector2D(clippedLine.EndPoint.X - clippedLine.StartPoint.X, clippedLine.EndPoint.Y - clippedLine.StartPoint.Y);
                        if (0 == Dot(N[i], D))
                        {
                            if (0 < Dot(N[i], new Vector2D(clippedLine.EndPoint.X - PE[i].X, clippedLine.EndPoint.Y - PE[i].Y)))
                            {
                                // discard
                                break;
                            }
                        }
                        else
                        {
                            double t = Dot(N[i], new Vector2D(clippedLine.StartPoint.X - PE[i].X, clippedLine.StartPoint.Y - PE[i].Y)) /
                                Dot(N[i], D) * (-1);
                            if(t >= 0 && t <= 1)
                            {
                                if (0 > Dot(N[i], D))   // PE
                                    tE = Math.Max(tE, t);
                                else    // PL
                                    tL = Math.Min(tL, t);
                            }
                        }
                    }

                    if(tE <= tL)
                    {
                        System.Drawing.Point PtE = new((int)((1 - tE) * clippedLine.StartPoint.X + tE * clippedLine.EndPoint.X),
                            (int)((1 - tE) * clippedLine.StartPoint.Y + tE * clippedLine.EndPoint.Y));
                        System.Drawing.Point PtL = new((int)((1 - tL) * clippedLine.StartPoint.X + tL * clippedLine.EndPoint.X),
                            (int)((1 - tL) * clippedLine.StartPoint.Y + tL * clippedLine.EndPoint.Y));
                        clippingResult.Pixels.AddRange(new DDALine(PtE, PtL).Pixels);
                    }
                    hasBroken = false;


                    // Calculating P1 - P0
                    //int[] P1_P0 = new int[]
                    //{
                    //clippedLine.EndPoint.X - clippedLine.StartPoint.X,
                    //clippedLine.EndPoint.Y - clippedLine.StartPoint.Y
                    //};


                    //// Initializing all values of P0 - PEi
                    //int[][] P0_PEi = new int[clippingEdges.Count][];

                    //// Calculating the values of P0 - PEi for all edges
                    //for (int i = 0; i < clippingEdges.Count; i++)
                    //{
                    //    P0_PEi[i] = new int[2];
                    //    // Calculating PEi - P0, so that the
                    //    // denominator won't have to multiply by -1
                    //    P0_PEi[i][0] = PE[i].X - clippedLine.StartPoint.X;

                    //    // while calculating 't'
                    //    P0_PEi[i][1] = PE[i].Y - clippedLine.StartPoint.Y;
                    //}
                }
            }

            return clippingResult;
        }


        #region Testing

        private void PutTestPolygonOnCanvasAndFill()
        {
            List<System.Drawing.Point> pts = new List<System.Drawing.Point>
            {
                new System.Drawing.Point(120, 120),
                new System.Drawing.Point(130, 220),
                new System.Drawing.Point(140, 180),
                new System.Drawing.Point(160, 300),
                new System.Drawing.Point(200, 100),
            };
            Polygon testPolygon = new(pts, System.Drawing.Color.Black);
            drawnObjects.Add(testPolygon);
            PutObjectOnCanvas(testPolygon, testPolygon.Color);
            ObjectsListBox.SelectedIndex = 0;
            FillBt.RaiseEvent(new(Button.ClickEvent));
        }

        private void PutTestPolygon2OnCanvasAndFill()
        {
            List<System.Drawing.Point> pts2 = new List<System.Drawing.Point>
            {
                new System.Drawing.Point(300, 120),
                new System.Drawing.Point(300, 140),
                new System.Drawing.Point(340, 140),
                new System.Drawing.Point(340, 120)
            };
            Polygon test2Polygon = new(pts2, System.Drawing.Color.Black);
            drawnObjects.Add(test2Polygon);
            PutObjectOnCanvas(test2Polygon, test2Polygon.Color);
            ObjectsListBox.SelectedIndex = 0;
            FillBt.RaiseEvent(new(Button.ClickEvent));
        }

        private void RunTestsBt_Click(object sender, RoutedEventArgs e)
        {
            // Testing
            //PutTestPolygonOnCanvasAndFill();
            //Application.Current.DoEvents();
            //Thread.Sleep(200);
            //DeleteBt.RaiseEvent(new(Button.ClickEvent));


            //PutTestPolygon2OnCanvasAndFill();
            //Application.Current.DoEvents();
            //Thread.Sleep(200);
            //DeleteBt.RaiseEvent(new(Button.ClickEvent));


            //for (int i = 0; i < 20; i++)
            //{
            //    MidPointCircle perimeter = new(new((int)(Canvas.Width / 2), (int)(Canvas.Height / 2)), 200);
            //    Random rng = new(i);
            //    int currentSum = 0;
            //    int[] randAngles = GenerateRandomIntArray(minValue: 5, maxValue: (int)Math.Round((360f / (10 + i))), lenght: 10 + i);
            //    int[] cumRandAngles = randAngles.Select(i =>
            //    {
            //        currentSum += i;
            //        return currentSum;
            //    }).ToArray();
            //    List<System.Drawing.Point> randPoints = cumRandAngles.Select(angle => new System.Drawing.Point((int)(perimeter.Radius * Math.Cos(angle / 360f * 2 * Math.PI)) + perimeter.Center.X,
            //        (int)(perimeter.Radius * Math.Sin(angle / 360f * 2 * Math.PI) + perimeter.Center.Y))).ToList();
            //    Polygon newPoly = new(randPoints);
            //    drawnObjects.Add(newPoly);
            //    PutObjectOnCanvas(newPoly, newPoly.Color);
            //    ObjectsListBox.SelectedIndex = 0;
            //    FillBt.RaiseEvent(new(Button.ClickEvent));
            //    Application.Current.DoEvents();
            //    Thread.Sleep(200);
            //    DeleteBt.RaiseEvent(new(Button.ClickEvent));
            //}

            BasicClippingSetup();
        }

        private void BasicClippingSetup()
        {
            List<System.Drawing.Point> pts = new List<System.Drawing.Point>
            {
                new System.Drawing.Point(200, 200),
                new System.Drawing.Point(200, 400),
                new System.Drawing.Point(600, 400),
                new System.Drawing.Point(600, 200)
            };
            Polygon testPoly = new(pts, System.Drawing.Color.Black);
            drawnObjects.Add(testPoly);
            PutObjectOnCanvas(testPoly, testPoly.Color);
            ObjectsListBox.SelectedIndex = 0;
            Application.Current.DoEvents();

            ClippingBt.RaiseEvent(new(Button.ClickEvent));
            var clickPoint = Canvas.PointToScreen(new System.Windows.Point(400, 100));
            LeftMouseClick((int)clickPoint.X, (int)clickPoint.Y);
            clickPoint = Canvas.PointToScreen(new System.Windows.Point(400, 500));
            LeftMouseClick((int)clickPoint.X, (int)clickPoint.Y);
            clickPoint = Canvas.PointToScreen(new System.Windows.Point(700, 500));
            LeftMouseClick((int)clickPoint.X, (int)clickPoint.Y);
            clickPoint = Canvas.PointToScreen(new System.Windows.Point(700, 100));
            LeftMouseClick((int)clickPoint.X, (int)clickPoint.Y);
            clickPoint = Canvas.PointToScreen(new System.Windows.Point(410, 110));
            LeftMouseClick((int)clickPoint.X, (int)clickPoint.Y);
        }

        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        #endregion

    }
}


/*
 * Clipping usage steps:
 * 
 *  1. Select object to be clipped
 *  2. Click 'Clipping mode' button.
 *  3. Draw clipping rectangle.
 *  4. Drag handles of both rectangles to adjust clipping.
 *  
 */
