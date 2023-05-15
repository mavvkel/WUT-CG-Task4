using System.Runtime.CompilerServices;

namespace Extensions
{
    public static class Extensions
    {
        public static System.Drawing.Color? ToSystemDrawingColor(this System.Windows.Media.Color? color)
        {
            if (color.HasValue)
                return System.Drawing.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
            else
                return null;
        }
    }
}
