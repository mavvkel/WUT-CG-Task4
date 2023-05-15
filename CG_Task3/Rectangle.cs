using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CG_Task3
{
    internal class Rectangle : I2DPrimitive
    {
        private Point _startCornerPoint;
        private Point _endCornerPoint;
        private List<Point> _handlePoints;

        #region Constructors

        [JsonConstructor]
        public Rectangle(Point startCornerPoint, Point endCornerPoint, Color color)
        {
            _startCornerPoint = startCornerPoint;
            _endCornerPoint = endCornerPoint;
            _handlePoints = new List<Point>
            {
                _startCornerPoint,
                new Point(_startCornerPoint.X, _endCornerPoint.Y),
                _endCornerPoint,
                new Point(_endCornerPoint.X, _startCornerPoint.Y),
            };
            Pixels = CalculatePixels();
            Color = color;
        }

        #endregion

        #region Properties

        public List<Point> Pixels { get; private set; }

        public List<Point> HandlePoints
        {
            get
            {
                return _handlePoints;
            }
            set
            {
                _handlePoints = value;
                Pixels = CalculatePixels();
            }
        }

        List<Point> I2DPrimitive.HandlePoints
        {
            get
            {
                return _handlePoints;
            }
            set
            {
                _handlePoints = value;
                Pixels = CalculatePixels();
            }
        }

        public Color Color { get; set; }

        #endregion

        #region Helpers

        private List<Point> CalculatePixels()
        {
            List<Point> newPointCollection = new();
            for(int i = 0; i < _handlePoints.Count; i++)
            {
                DDALine segment = new(_handlePoints.ElementAt(i), _handlePoints.ElementAt((i + 1) % _handlePoints.Count));
                newPointCollection = newPointCollection.Concat(segment.Pixels).ToList();
            }
            
            return newPointCollection;
        }

        #endregion

        public override string ToString()
        {
            string objDesc = "Rectangle with points";
            foreach(Point point in _handlePoints)
            {
                objDesc = string.Concat(objDesc, " (", point.ToString(), "),");
            }
            return objDesc.Remove(objDesc.Length - 1);
        }
    }
}
