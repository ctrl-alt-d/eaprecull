using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Globalization;

namespace UI.ER.AvaloniaUI.Controls
{
    public partial class DateInput : UserControl
    {
        private bool _isUpdating = false;
        private const string DateFormat = "dd.MM.yyyy";
        private Avalonia.Controls.Calendar? _calendar;
        private Button? _calendarButton;
        private Button? _clearButton;
        private DateTime? _dateOnOpen;

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

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            _calendar = this.FindControl<Avalonia.Controls.Calendar>("DateCalendar");
            _calendarButton = this.FindControl<Button>("CalendarButton");
            _clearButton = this.FindControl<Button>("ClearButton");

            if (_calendar != null)
            {
                _calendar.AddHandler(PointerReleasedEvent, Calendar_PointerReleased, RoutingStrategies.Tunnel);
            }

            if (_calendarButton?.Flyout is Flyout flyout)
            {
                flyout.Opening += Flyout_Opening;
            }

            if (_clearButton != null)
            {
                _clearButton.Click += ClearButton_Click;
            }
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            if (_calendar != null)
            {
                _calendar.RemoveHandler(PointerReleasedEvent, Calendar_PointerReleased);
            }

            if (_calendarButton?.Flyout is Flyout flyout)
            {
                flyout.Opening -= Flyout_Opening;
            }

            if (_clearButton != null)
            {
                _clearButton.Click -= ClearButton_Click;
            }

            base.OnUnloaded(e);
        }

        private void ClearButton_Click(object? sender, RoutedEventArgs e)
        {
            DateText = string.Empty;
        }

        private void Flyout_Opening(object? sender, EventArgs e)
        {
            if (_calendar != null)
            {
                _isUpdating = true;
                try
                {
                    var date = ParseDateFromText();
                    _dateOnOpen = date;
                    _calendar.SelectedDate = date;
                    _calendar.DisplayDate = date ?? DateTime.Today;
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        private void Calendar_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isUpdating) return;

            // Deixem que el Calendar processi el clic i actualitzi SelectedDate
            Dispatcher.UIThread.Post(() =>
            {
                if (_calendar?.SelectedDate is DateTime selectedDate)
                {
                    // Només actuar si la data ha canviat respecte la que hi havia en obrir
                    if (_dateOnOpen.HasValue && _dateOnOpen.Value.Date == selectedDate.Date)
                    {
                        return;
                    }

                    _isUpdating = true;
                    try
                    {
                        DateText = selectedDate.ToString(DateFormat, CultureInfo.InvariantCulture);
                        _dateOnOpen = selectedDate;

                        if (_calendarButton?.Flyout is Flyout flyout)
                        {
                            flyout.Hide();
                        }
                    }
                    finally
                    {
                        _isUpdating = false;
                    }
                }
            }, DispatcherPriority.Input);
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
