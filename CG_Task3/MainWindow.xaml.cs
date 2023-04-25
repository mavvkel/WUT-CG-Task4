using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Threading.Channels;
using System.ComponentModel;

namespace CG_Task3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // statics
        static readonly DrawingAttributes selectDA = new();
        DrawingAttributes drawingDA = new();

        // flags
        bool isDrawingLine = false;
        bool currentlyDrawing = false;
        bool isDrawingTask = false;

        // buffers
        StylusPoint? pointBuffer = null;
        I2DPrimitive? lastSelected = null;
        List<Thumb>? currentSelectionThumbs = null;

        // collections
        List<I2DPrimitive> drawnObjects;

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

            selectDA.Color = Colors.CornflowerBlue;
        }

        private void LineBt_Click(object sender, RoutedEventArgs e)
        {
            isDrawingLine = !isDrawingLine;
            StatusMsgLabel.Content = $"Line drawing = {isDrawingLine}";
            if (isDrawingLine)
            {
                LineBt.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            }
            else
            {
                LineBt.Foreground = (SolidColorBrush)App.Current.Resources["DarkThemeFGBrush"];
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pointBuffer = new StylusPoint(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
            pointBuffer = new StylusPoint(Math.Round(pointBuffer.Value.X), Math.Round(pointBuffer.Value.Y));
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
            else
                StatusMsgLabel.Content = $"Button down recorded but no drawing option selected";
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentlyDrawing)
            {
                if (isDrawingLine)
                {
                    currentlyDrawing = false;
                    StylusPoint endPoint = new (e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
                    endPoint = new StylusPoint(Math.Round(endPoint.X), Math.Round(endPoint.Y));
                    StatusMsgLabel.Content = $"Button up recorded, coords = ({endPoint.X},{endPoint.Y})";

                    Debug.Assert(pointBuffer.HasValue);
                    DDALine newLine = new(pointBuffer.Value, endPoint);
                    DrawPoints(newLine.Pixels);
                    drawnObjects.Add(newLine);
                    ObjectsListBox.UpdateLayout();
                    ObjectsListBox.Items.Refresh();
                }
                else if (isDrawingTask)
                {
                    currentlyDrawing = false;
                    StylusPoint endPoint = new(e.GetPosition(Canvas).X, e.GetPosition(Canvas).Y);
                    endPoint = new StylusPoint(Math.Round(endPoint.X), Math.Round(endPoint.Y));
                    StatusMsgLabel.Content = $"Button up recorded, coords = ({endPoint.X},{endPoint.Y})";

                    Debug.Assert(pointBuffer.HasValue);
                    TaskShape newTaskShape = new(pointBuffer.Value, endPoint);
                    DrawPoints(newTaskShape.ShapePixels);
                }

            }
            else
                StatusMsgLabel.Content = $"Button up recorded but no drawing option selected";
        }

        private void DrawPoints(StylusPointCollection points)
        {
            Stroke stroke = new(points, Canvas.DefaultDrawingAttributes);
            Canvas.Strokes.Add(stroke);
        }

        private void ObjectsListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (null != ObjectsListBox.SelectedItem)
            {
                // Unselect previously selected items
                if (null != lastSelected)
                    ChangeCanvasStroke(lastSelected, drawingDA);

                // Erase previous edit handles
                if (null != currentSelectionThumbs)
                {
                    foreach (Thumb thumbChild in currentSelectionThumbs)
                        Canvas.Children.Remove(thumbChild);

                    currentSelectionThumbs.Clear();
                }

                // Selecting the new stroke
                I2DPrimitive? selectedObject = (I2DPrimitive)ObjectsListBox.SelectedItem;

                //Stroke selectedStroke= Canvas.Strokes.Where(stroke => stroke.StylusPoints.SequenceEqual(selectedObject.Pixels)).First();
                //StrokeCollection selectedStrokeCollection = new()
                //{
                //    selectedStroke
                //};
                //Canvas.Select(selectedStrokeCollection);
                StatusMsgLabel.Content = $"Object selected = {selectedObject}";

                ChangeCanvasStroke(selectedObject, selectDA);
                currentSelectionThumbs = new List<Thumb>();
                foreach (StylusPoint handlePoint in selectedObject.HandlesPoints)
                {
                    currentSelectionThumbs.Add(new Thumb());
                    Thumb current = currentSelectionThumbs.Last();
                    current.Tag = new Tuple<I2DPrimitive, int>(selectedObject, selectedObject.HandlesPoints.IndexOf(handlePoint));
                    Canvas.Children.Add(current);
                    var thumbTemplate = (ControlTemplate)Application.Current.MainWindow.Resources["EditHandleTemplate"];
                    current.Template = thumbTemplate;
                    current.DragDelta += OnDragDelta;
                    InkCanvas.SetLeft(current, handlePoint.X - 5); // 5 is 1/2 of Width in EditHandleTemplate
                    InkCanvas.SetTop(current, handlePoint.Y - 5); // 5 is 1/2 of Height in EditHandleTemplate
                }

                lastSelected = selectedObject;
                DeleteBt.IsEnabled = true;
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
            Debug.Assert(Canvas.Strokes.Where(stroke => stroke.StylusPoints.SequenceEqual(lastSelected.Pixels)).Count() == 1);
            Canvas.Strokes.Remove(
                Canvas.Strokes.Where(stroke => stroke.StylusPoints.SequenceEqual(lastSelected.Pixels))
                .First());
            drawnObjects.Remove(lastSelected);
            lastSelected = null;
            ObjectsListBox.Items.Refresh();
            //ObjectsListBox.UpdateLayout();
            if (null != currentSelectionThumbs)
            {
                foreach (Thumb thumbChild in currentSelectionThumbs)
                    Canvas.Children.Remove(thumbChild);

                currentSelectionThumbs.Clear();
            }
            DeleteBt.IsEnabled = false;
        }

        private void ChangeCanvasStroke(I2DPrimitive drawnObject, DrawingAttributes newDA)
        {
            Debug.Assert(Canvas.Strokes.Where(stroke => stroke.StylusPoints.SequenceEqual(drawnObject.Pixels)).Count() == 1);

            Stroke oldStroke = Canvas.Strokes.Where(stroke => stroke.StylusPoints.SequenceEqual(drawnObject.Pixels)).First();
            Stroke newStroke = oldStroke.Clone();
            newStroke.DrawingAttributes = newDA;

            StrokeCollection newStrokeCollection = new()
            {
                newStroke
            };
            Canvas.Strokes.Replace(oldStroke, newStrokeCollection);
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb handle = (Thumb)sender;

            double newX = Math.Round(InkCanvas.GetLeft(handle) + e.HorizontalChange);
            double newY = Math.Round(InkCanvas.GetTop(handle) + e.VerticalChange);

            // Move the handle
            InkCanvas.SetLeft(handle, newX - 5);
            InkCanvas.SetTop(handle, newY - 5);


            //I2DPrimitive selectedObject = ((Tuple<I2DPrimitive, int>)handle.Tag).Item1;
            //StylusPointCollection oldPointCollection = new(selectedObject.Pixels);
            //int pointIndex = ((Tuple<I2DPrimitive, int>)handle.Tag).Item2;

            // Update the drawn object
            //Debug.Assert(drawnObjects.ElementAt(pointIndex) == selectedObject);
            //StylusPointCollection updatedPointCollection = new(selectedObject.HandlesPoints);
            //updatedPointCollection.RemoveAt(pointIndex);
            //updatedPointCollection.Insert(pointIndex, new StylusPoint(newX, newY));
            //selectedObject.HandlesPoints = updatedPointCollection;

            //// Update the Canvas stroke
            //Stroke updatedObjectStroke = new(updatedPointCollection);
            //StrokeCollection updatedStrokeCollection = new()
            //{
            //    updatedObjectStroke
            //};
            //Canvas.Strokes.Replace(
            //    Canvas.Strokes.Where(stroke => stroke.StylusPoints.SequenceEqual(oldPointCollection))
            //    .First(), updatedStrokeCollection);

            //Canvas.Strokes.Replace(
            //    Canvas.Strokes.Where(stroke => stroke.StylusPoints.SequenceEqual(oldPointCollection))
            //    .First(), updatedStrokeCollection);

            //selectedObject.HandlesPoints = movedPoints;
            //UpdateCanvasStrokes();
            //StylusPoint selectedObject.Pixels.Where(point => (point.X == movedPoint.X) && (point.Y == movedPoint.Y)).First().X = 
        }

        private void UpdateCanvasStrokes()
        {
            Canvas.Strokes.Clear();
            foreach (I2DPrimitive drawnObject in drawnObjects)
                DrawPoints(drawnObject.Pixels);
        }
    }
}
