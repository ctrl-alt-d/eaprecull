using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Styles.Controls;
using System.Threading.Tasks;

namespace UI.ER.AvaloniaUI.Helpers
{
    public static class ConfirmationDialog
    {
        public static async Task<bool> Show(Window owner, string message, string title = "Confirmació")
        {
            var result = false;

            var window = new Window
            {
                Title = title,
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false,
                SystemDecorations = SystemDecorations.BorderOnly
            };

            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20, 20, 20, 10),
                FontSize = 14
            };

            var yesButton = new Button
            {
                Content = "Sí, esborrar",
                Margin = new Thickness(5),
                MinWidth = 100,
                Classes = { "Flat" },
                Foreground = new SolidColorBrush(Color.Parse("#D32F2F"))
            };
            yesButton.Click += (s, e) =>
            {
                result = true;
                window.Close();
            };

            var noButton = new Button
            {
                Content = "Cancel·lar",
                Margin = new Thickness(5),
                MinWidth = 100,
                Classes = { "Flat" }
            };
            noButton.Click += (s, e) =>
            {
                result = false;
                window.Close();
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 10, 20, 20),
                Children = { noButton, yesButton }
            };

            var mainPanel = new StackPanel
            {
                Children = { messageText, buttonPanel },
                VerticalAlignment = VerticalAlignment.Center
            };

            window.Content = mainPanel;

            await window.ShowDialog(owner);

            return result;
        }
    }
}
