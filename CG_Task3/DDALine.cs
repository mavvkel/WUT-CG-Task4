using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Newtonsoft.Json;

namespace CG_Task3
{

    public class DDALine : I2DPrimitive
    {
        private System.Drawing.Point _startPoint;
        private System.Drawing.Point _endPoint;
        private List<System.Drawing.Point> _handlePoints;
        private int _brushThickness;

        #region Constructors

        public DDALine(System.Drawing.Point startPoint, System.Drawing.Point endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _handlePoints = new()
            {
                _startPoint,
                _endPoint
            };
            Pixels = CalculatePixels();
            Color = System.Drawing.Color.Black;
            BrushThickness = 1;
        }

        [JsonConstructor]
        public DDALine(System.Drawing.Point startPoint, System.Drawing.Point endPoint, System.Drawing.Color color, int brushThickness = 1)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            _handlePoints = new()
            {
                _startPoint,
                _endPoint
            };
            Pixels = CalculatePixels();
            Color = color;
            BrushThickness = brushThickness;
        }

        #endregion

        #region Properties

        public List<System.Drawing.Point> Pixels { get; private set; }

        public System.Drawing.Point StartPoint
        {
            get 
            {
                return _startPoint;
            }
            set
            {
                _startPoint = value;
                Pixels = CalculatePixels();
                UpdateHandlePoints();
            }
        }

        public System.Drawing.Point EndPoint 
        {
            get 
            {
                return _endPoint;
            }
            set
            {
                _endPoint = value;
                Pixels = CalculatePixels();
                UpdateHandlePoints();
            }
        }

        public List<System.Drawing.Point> HandlePoints
        {
            get
            {
                return _handlePoints;
            }
            set
            {
                Debug.Assert(value.Count == 2);
                _handlePoints = value;
                _startPoint = _handlePoints.ElementAt(0);
                _endPoint = _handlePoints.ElementAt(1);
                Pixels = CalculatePixels();
            }
        }

        List<System.Drawing.Point> I2DPrimitive.HandlePoints
        {
            get {
                return _handlePoints;
            }
            set
            {
                Debug.Assert(value.Count == 2);
                _handlePoints = value;
                _startPoint = _handlePoints.ElementAt(0);
                _endPoint = _handlePoints.ElementAt(1);
                Pixels = CalculatePixels();
            }
        }

        public Color Color { get; set; }

        public int BrushThickness
        {
            get 
            {
                return _brushThickness;
            }
            set
            {
                _brushThickness = value;
                Pixels = CalculatePixels();
            }
        }

        #endregion

        #region Helpers

        private List<System.Drawing.Point> CalculatePixels()
        {
            System.Drawing.Point first = (_startPoint.X < _endPoint.X) ? _startPoint : _endPoint; 
            System.Drawing.Point second = (_startPoint.X < _endPoint.X) ? _endPoint : _startPoint;

            int dy = second.Y - first.Y;
            int dx = second.X - first.X;
            int steps;

            if (Math.Abs(dx) > Math.Abs(dy))
                steps = Math.Abs(dx);
            else
                steps = Math.Abs(dy);

            double x_inc = dx / (double)steps;
            double y_inc = dy / (double)steps;

            double x = first.X;
            double y = first.Y;

            List<System.Drawing.Point> newPointCollection = new();
            for (int i = 0; i <= steps; i++)
            {
                newPointCollection.Add(new System.Drawing.Point((int)Math.Round(x), (int)Math.Round(y)));
                for(int j = 2; j <= _brushThickness; j++)
                {
                    MidPointCircle circle = new(new System.Drawing.Point((int)Math.Round(x), (int)Math.Round(y)), j);
                    newPointCollection = newPointCollection.Union(circle.Pixels).ToList();
                }
                x += x_inc;
                y += y_inc;
            }

            return newPointCollection;
        }

        private void UpdateHandlePoints()
        {
            _handlePoints = new()
            {
                _startPoint,
                _endPoint
            };
        }

        #endregion

        public override string ToString()
        {
            return $"Line from ({StartPoint.X},{StartPoint.Y}) to ({EndPoint.X},{EndPoint.Y})";
        }
    }
}
