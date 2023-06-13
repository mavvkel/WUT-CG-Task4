using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;

namespace CG_Task3
{
    internal class MidPointCircle : I2DPrimitive
    {
        private System.Drawing.Point _center;
        private int _radius;
        private System.Drawing.Point _radiusHandlePoint;
        private List<System.Drawing.Point> _handlePoints;

        #region Constructors

        public MidPointCircle(System.Drawing.Point center, int radius)
        {
            _center = center;
            _radius = radius;
            _radiusHandlePoint = new(center.X + radius, center.Y);
            _handlePoints = new List<Point> { 
                _center,
                _radiusHandlePoint
            };

            Pixels = CalculatePixels();
            Color = System.Drawing.Color.Black;
        }

        [JsonConstructor]
        public MidPointCircle(System.Drawing.Point center, int radius, System.Drawing.Color color)
        {
            _center = center;
            _radius = radius;
            _radiusHandlePoint = new(center.X + radius, center.Y);
            _handlePoints = new List<Point> { 
                _center,
                _radiusHandlePoint
            };
            Pixels = CalculatePixels();
            Color = color;
        }

        #endregion

        #region Properties

        public int Radius 
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                Pixels = CalculatePixels();
            }
        }

        public System.Drawing.Point Center
        {
            get
            {
                return _center;
            }
            set 
            {
                _center = value;
                Pixels = CalculatePixels();
            }
        }

        public List<System.Drawing.Point> Pixels { get; private set; }

        List<Point> I2DPrimitive.HandlePoints
        {
            get {
                return _handlePoints;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value.Count == 2);
                _handlePoints = value;
                _center = _handlePoints.ElementAt(0);
                _radiusHandlePoint = _handlePoints.ElementAt(1);
                _radius = (int)Math.Sqrt(Math.Pow(_center.X - _radiusHandlePoint.X, 2) + Math.Pow(_center.Y - _radiusHandlePoint.Y, 2));
                Pixels = CalculatePixels();
            }
        }

        public Color Color { get; set; }

        public int BrushThickness
        {
            get
            {
                return 1;
            }
            set { }
        }

        public Point CenterHandlePoint => Center;

        #endregion

        #region Helpers

        private List<System.Drawing.Point> CalculatePixels()
        {
            int x = _radius, y = 0;

            List<System.Drawing.Point> newPointCollection = new()
            {
                // Adding the initial point on the axes
                new(x + _center.X, y + _center.Y)
            };

            // When radius is zero only a single
            // point will be printed
            if (_radius > 0)
            {
                newPointCollection.Add(new(x + _center.X, -y + _center.Y));
                newPointCollection.Add(new(y + _center.X, x + _center.Y));
                newPointCollection.Add(new(-y + _center.X, x + _center.Y));
            }

            // Initialising the value of P
            int P = 1 - _radius;
            while (x > y)
            {
                y++;

                // Mid-point is inside or on the perimeter
                if (P <= 0)
                    P = P + 2 * y + 1;
                // Mid-point is outside the perimeter
                else
                {
                    x--;
                    P = P + 2 * y - 2 * x + 1;
                }

                // All the perimeter points have already been printed
                if (x < y)
                    break;

                // Printing the generated point and its reflection
                // in the other octants after translation
                newPointCollection.Add(new(x + _center.X, y + _center.Y));
                newPointCollection.Add(new(-x + _center.X, y + _center.Y));
                newPointCollection.Add(new(x + _center.X, -y + _center.Y));
                newPointCollection.Add(new(-x + _center.X, -y + _center.Y));

                // If the generated point is on the line x = y then
                // the perimeter newPointCollection have already been printed
                if (x != y)
                {
                    newPointCollection.Add(new(y + _center.X, x + _center.Y));
                    newPointCollection.Add(new(-y + _center.X, x + _center.Y));
                    newPointCollection.Add(new(y + _center.X, -x + _center.Y));
                    newPointCollection.Add(new(-y + _center.X, -x + _center.Y));
                }
            }

            return newPointCollection;
        }

        #endregion

        public override string ToString()
        {
            return $"Circle at ({_center.X},{_center.Y}) with radius of {_radius}";
        }
    }
}
