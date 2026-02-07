using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace UI.ER.AvaloniaUI.Helpers
{
    public sealed class WindowHelper
    {
        public static readonly AttachedProperty<bool> ClampToWorkingAreaProperty =
            AvaloniaProperty.RegisterAttached<WindowHelper, TopLevel, bool>("ClampToWorkingArea");

        public static void SetClampToWorkingArea(TopLevel element, bool value)
            => element.SetValue(ClampToWorkingAreaProperty, value);

        public static bool GetClampToWorkingArea(TopLevel element)
            => element.GetValue(ClampToWorkingAreaProperty);

        static WindowHelper()
        {
            ClampToWorkingAreaProperty.Changed.Subscribe(args =>
            {
                if (args.Sender is TopLevel tl)
                {
                    var enabled = tl.GetValue(ClampToWorkingAreaProperty);
                    if (enabled)
                        tl.Opened += Tl_Opened;
                    else
                        tl.Opened -= Tl_Opened;
                }
            });
        }

        private static void Tl_Opened(object? sender, EventArgs e)
        {
            try
            {
                if (sender is Window w)
                {
                    var screens = w.Screens;
                    var primary = screens?.Primary;
                    var work = primary?.WorkingArea;
                    if (work != null && primary != null)
                    {
                        var scaling = primary.Scaling;
                        double workHeight = work.Value.Size.Height / scaling;
                        double workWidth = work.Value.Size.Width / scaling;
                        double workTop = work.Value.Position.Y / scaling;
                        double workLeft = work.Value.Position.X / scaling;

                        if (!double.IsNaN(w.Height) && w.Height > workHeight)
                            w.Height = workHeight;
                        if (!double.IsNaN(w.Width) && w.Width > workWidth)
                            w.Width = workWidth;

                        // Move window inside working area. Use Position (PixelPoint) when available
                        try
                        {
                            var pos = w.Position;
                            double curLeft = pos.X / scaling;
                            double curTop = pos.Y / scaling;

                            double newTop = curTop;
                            double newLeft = curLeft;

                            if (curTop < workTop) newTop = workTop;
                            if (curLeft < workLeft) newLeft = workLeft;

                            if (newTop + w.Height > workTop + workHeight)
                                newTop = (workTop + workHeight) - w.Height;
                            if (newLeft + w.Width > workLeft + workWidth)
                                newLeft = (workLeft + workWidth) - w.Width;

                            w.Position = new PixelPoint((int)(newLeft * scaling), (int)(newTop * scaling));
                        }
                        catch
                        {
                            // If Position not available on this platform/version, ignore
                        }
                    }
                }
            }
            catch
            {
                // swallow platform-specific errors
            }
        }
    }
}
