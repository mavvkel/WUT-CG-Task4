using System;
using System.Windows;
using System.Windows.Threading;

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

        public static void DoEvents(this Application? app)
        {
            app?.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

    }
}
