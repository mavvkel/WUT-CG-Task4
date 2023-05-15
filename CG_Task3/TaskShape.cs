using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CG_Task3
{
    class TaskShape
    {
        public DDALine BaseLine { get; set; }

        public List<System.Drawing.Point> ShapePixels { get; private set; }


        public TaskShape(System.Drawing.Point start, System.Drawing.Point end)
        {
            BaseLine = new DDALine(start, end);
            ShapePixels = CalculateShapePixels();
        }

        private List<System.Drawing.Point> CalculateShapePixels()
        {
            int N = 3;      // adjustable number of half circles
            List<System.Drawing.Point> pixels = BaseLine.Pixels;

            if (BaseLine.StartPoint.X != BaseLine.EndPoint.X)
            {
                double lenght = Math.Sqrt(Math.Pow(BaseLine.StartPoint.X - BaseLine.EndPoint.X, 2) + Math.Pow(BaseLine.StartPoint.Y - BaseLine.EndPoint.Y, 2));
                double lineAngle = Math.Atan((BaseLine.StartPoint.Y - BaseLine.EndPoint.Y) / (BaseLine.StartPoint.X - BaseLine.EndPoint.X));
                double radius = lenght / (N * 2);

                List<Point> centers = new();
                for (int i = 1; i <= N; i++)
                {
                     List<System.Drawing.Point> circle = ProduceAngledHalfCirclePoints(new Point(BaseLine.StartPoint.X + Math.Sign(lineAngle) * (i + 0.5f) * lenght * Math.Cos(lineAngle),
                                                                                    BaseLine.StartPoint.Y + Math.Sign(lineAngle) * (i + 0.5f) * lenght * Math.Sin(lineAngle)),
                                                                                radius,
                                                                                lineAngle);
                    pixels.Concat(circle);
                }
            }

            return pixels;
        }

        //public override string ToString()
        //{
        //    return $"Line from ({StartPoint.X},{StartPoint.Y}) to ({EndPoint.X},{EndPoint.Y})";
        //}


        private List<System.Drawing.Point> ProduceAngledHalfCirclePoints(Point center, double radius, double angle)
        {
            List<System.Drawing.Point> points = new();
            midPointCircleDraw((int)center.X, (int)center.Y, (int)radius, points);

            return points;
        }

        void midPointCircleDraw(int x_centre, int y_centre, int r, List<System.Drawing.Point> points)
        {
            int x = r, y = 0;

            // Printing the initial point on the axes
            // after translation
            points.Add(new System.Drawing.Point(x + x_centre, y + y_centre));

            // When radius is zero only a single
            // point will be printed
            if (r > 0)
            {
                points.Add(new System.Drawing.Point(x + x_centre, -y + y_centre));
                points.Add(new System.Drawing.Point(y + x_centre, x + y_centre));
                points.Add(new System.Drawing.Point(-y + x_centre, x + y_centre));
            }

            // Initialising the value of P
            int P = 1 - r;
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
                points.Add(new System.Drawing.Point(x + x_centre, y + y_centre));
                points.Add(new System.Drawing.Point(-x + x_centre, y + y_centre));
                points.Add(new System.Drawing.Point(x + x_centre, -y + y_centre));
                points.Add(new System.Drawing.Point(-x + x_centre, -y + y_centre));

                // If the generated point is on the line x = y then
                // the perimeter points have already been printed
                if (x != y)
                {
                    points.Add(new System.Drawing.Point(y + x_centre, x + y_centre));
                    points.Add(new System.Drawing.Point(-y + x_centre, x + y_centre));
                    points.Add(new System.Drawing.Point(y + x_centre, -x + y_centre));
                    points.Add(new System.Drawing.Point(-y + x_centre, -x + y_centre));
                }
            }
        }


    }
}
