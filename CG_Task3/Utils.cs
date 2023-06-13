using System;
using System.Collections.Generic;

namespace CG_Task3
{
    static class Utils
    {
        public static System.Drawing.Point CalculateCentroid(List<System.Drawing.Point> points)
        {
            float area = 0.0f;
            int n = points.Count;
            for (int i = 0; i < n; i++)
                area += points[i].X * points[(i + 1) % n].Y - points[(i + 1) % n].X * points[i].Y;

            area *= 0.5f;

            float cx = 0.0f;
            float cy = 0.0f;

            for (int i = 0; i < n; i++)
                cx += (points[i].X + points[(i + 1) % n].X) * (points[i].X * points[(i + 1) % n].Y - points[(i + 1) % n].X * points[i].Y);
            cx /= 6 * area;
            for (int i = 0; i < n; i++)
                cy += (points[i].Y + points[(i + 1) % n].Y) * (points[i].X * points[(i + 1) % n].Y - points[(i + 1) % n].X * points[i].Y);
            cy /= 6 * area;

            return new((int)Math.Round(cx), (int)Math.Round(cy));
        }

    }

    enum Nums
    {
        CenterHandleTag = 999
    }
}
