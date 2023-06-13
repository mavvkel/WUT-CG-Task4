using System;
using System.Collections.Generic;
using System.Drawing;

namespace CG_Task3
{
    internal class Blob : I2DPrimitive
    {

        #region Constructors
        
        public Blob(List<System.Drawing.Point> points)
        {
            Pixels = points;
        }

        #endregion

        public List<Point> Pixels { get; set; }

        public List<Point> HandlePoints { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Color Color { get; set; }

        public int BrushThickness
        {
            get
            {
                return 0;
            }
            set
            {
                
            }
        }

        public Point CenterHandlePoint => throw new NotImplementedException();
    }
}
