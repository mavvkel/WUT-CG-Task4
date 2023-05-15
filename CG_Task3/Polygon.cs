using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CG_Task3
{
    public class Polygon : I2DPrimitive
    {
        private List<System.Drawing.Point> _handlePoints;

        #region Constructors

        public Polygon(List<System.Drawing.Point> handlePoints)
        {
            _handlePoints = new(handlePoints);
            Pixels = CalculatePixels();
            Color = System.Drawing.Color.Black;
        }

        [JsonConstructor]
        public Polygon(List<System.Drawing.Point> handlePoints, System.Drawing.Color color)
        {
            _handlePoints = new(handlePoints);
            Pixels = CalculatePixels();
            Color = color;
        }

        #endregion

        #region Properties

        public List<Point> Pixels { get; private set; }

        public List<System.Drawing.Point> HandlePoints
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

        List<System.Drawing.Point> I2DPrimitive.HandlePoints
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

        private List<System.Drawing.Point> CalculatePixels()
        {
            List<System.Drawing.Point> newPointCollection = new();
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
            string objDesc = "Polygon with points";
            foreach(Point point in _handlePoints)
            {
                objDesc = string.Concat(objDesc, " (", point.ToString(), "),");
            }
            return objDesc.Remove(objDesc.Length - 1);
        }
    }
}
