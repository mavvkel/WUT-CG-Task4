using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Security.Cryptography.Pkcs;
using System.Windows.Input;
using System.Linq;

namespace CG_Task3
{

    public class DDALine : I2DPrimitive
    {
        private StylusPoint _startPoint;
        private StylusPoint _endPoint;
        private StylusPointCollection _handlePoints;

        public StylusPointCollection Pixels { get; private set; }
        public StylusPoint StartPoint
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

        public StylusPoint EndPoint 
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

        public StylusPointCollection HandlesPoints
        {
            get {
                return _handlePoints;
            }
            set
            {
                _handlePoints = value;
                _startPoint = _handlePoints.ElementAt(0);
                _endPoint = _handlePoints.ElementAt(1);
                Pixels = CalculatePixels();
            }
        }

        StylusPointCollection I2DPrimitive.HandlesPoints
        {
            get {
                return _handlePoints;
            }
            set
            {
                _handlePoints = value;
                _startPoint = _handlePoints.ElementAt(0);
                _endPoint = _handlePoints.ElementAt(1);
                Pixels = CalculatePixels();
            }
        }

        public DDALine(StylusPoint start, StylusPoint end)
        {
            _startPoint = start;
            _endPoint = end;
            _handlePoints = new StylusPointCollection()
            {
                _startPoint,
                _endPoint
            };
            Pixels = CalculatePixels();
        }

        private StylusPointCollection CalculatePixels()
        {
            double dy = EndPoint.Y - StartPoint.Y;
            double dx = EndPoint.X - StartPoint.X;
            double m = dy / dx;
            double y = StartPoint.Y;
            int x = (int)StartPoint.X;
            int endX = (int)EndPoint.X;

            if (EndPoint.X < StartPoint.X)
            {
                x = (int)EndPoint.X;
                y = (int)EndPoint.Y;
                endX = (int)StartPoint.X;
            }

            StylusPointCollection newPointCollection = new();
            for (; x <= endX; ++x)
            {
                newPointCollection.Add(new StylusPoint(x, Math.Round(y)));
                y += m;
            }

            return newPointCollection;
        }

        private void UpdateHandlePoints()
        {
            _handlePoints = new StylusPointCollection()
            {
                _startPoint,
                _endPoint
            };
        }

        public override string ToString()
        {
            return $"Line from ({StartPoint.X},{StartPoint.Y}) to ({EndPoint.X},{EndPoint.Y})";
        }

    }
}
