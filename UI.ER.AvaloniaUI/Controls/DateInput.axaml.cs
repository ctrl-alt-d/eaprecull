using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System;
using System.Globalization;

namespace UI.ER.AvaloniaUI.Controls
{
    public partial class DateInput : UserControl
    {
        private bool _isUpdating = false;
        private const string DateFormat = "d.M.yyyy";
        private Avalonia.Controls.Calendar? _calendar;
        private Button? _calendarButton;

        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<DateInput, string>(nameof(Label), "Data");

        public static readonly StyledProperty<string> DateTextProperty =
            AvaloniaProperty.Register<DateInput, string>(nameof(DateText), string.Empty, defaultBindingMode: BindingMode.TwoWay);

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string DateText
        {
            get => GetValue(DateTextProperty);
            set => SetValue(DateTextProperty, value);
        }

        public DateInput()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
        }

        protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            
            _calendar = this.FindControl<Avalonia.Controls.Calendar>("DateCalendar");
            _calendarButton = this.FindControl<Button>("CalendarButton");
            
            if (_calendar != null)
            {
                _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
            }
            
            if (_calendarButton?.Flyout is Flyout flyout)
            {
                flyout.Opening += Flyout_Opening;
            }
        }

        protected override void OnUnloaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_calendar != null)
            {
                _calendar.SelectedDatesChanged -= Calendar_SelectedDatesChanged;
            }
            
            if (_calendarButton?.Flyout is Flyout flyout)
            {
                flyout.Opening -= Flyout_Opening;
            }
            
            base.OnUnloaded(e);
        }

        private void Flyout_Opening(object? sender, EventArgs e)
        {
            // Sincronitzar el calendari amb el text actual abans d'obrir
            if (_calendar != null)
            {
                var date = ParseDateFromText();
                _calendar.SelectedDate = date;
                _calendar.DisplayDate = date ?? DateTime.Today;
            }
        }

        private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating) return;
            
            _isUpdating = true;
            try
            {
                if (_calendar?.SelectedDate is DateTime selectedDate)
                {
                    DateText = selectedDate.ToString(DateFormat, CultureInfo.InvariantCulture);
                    
                    // Tanquem el flyout quan es selecciona una data
                    if (_calendarButton?.Flyout is Flyout flyout)
                    {
                        flyout.Hide();
                    }
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private DateTime? ParseDateFromText()
        {
            var text = DateText?.Trim() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            
            if (DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, 
                DateTimeStyles.None, out var result))
            {
                return result;
            }
            
            return null;
        }
    }
}
