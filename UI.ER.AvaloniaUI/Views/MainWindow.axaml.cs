using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using Material.Styles;
using UI.ER.AvaloniaUI.Pages;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Views
{
    public class MainWindow : Window
    {
        #region Control fields
        private ToggleButton NavDrawerSwitch = default!;
        private ListBox DrawerList = default!;
        private Carousel PageCarousel = default!;
        private Grid mainScroller = default!;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            this.AttachDevTools(KeyGesture.Parse("Shift+F12"));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            #region Control getter and event binding
            NavDrawerSwitch = this.Get<ToggleButton>(nameof(NavDrawerSwitch));

            DrawerList = this.Get<ListBox>(nameof(DrawerList));
            DrawerList.PointerReleased += DrawerSelectionChanged ;
            DrawerList.KeyUp += DrawerList_KeyUp;

            PageCarousel = this.Get<Carousel>(nameof(PageCarousel));

            mainScroller = this.Get<Grid>(nameof(mainScroller));
            #endregion
        }

        private void DrawerList_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
                DrawerSelectionChanged(sender, null);
        }

        public void DrawerSelectionChanged(object? sender, PointerReleasedEventArgs? args)
        {
            var listBox = sender as ListBox;
            if (!listBox!.IsFocused && !listBox.IsKeyboardFocusWithin)
                return;
            try
            {
                PageCarousel.SelectedIndex = listBox.SelectedIndex;
                //mainScroller.Offset = Vector.Zero;
                //mainScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    // listBox.SelectedIndex == 5 ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;

            }
            catch
            {
            }
            NavDrawerSwitch.IsChecked = false;
        }

        private void TemplatedControl_OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            SnackbarHost.Post("Welcome to demo of Material.Avalonia!");
        }

        private void Centre_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new CentreSetWindow()
            {
                DataContext = new CentreSetViewModel()
            };

            w.ShowDialog(this);
        }

        private void Etapa_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new EtapaSetWindow()
            {
                DataContext = new EtapaSetViewModel()
            };

            w.ShowDialog(this);
        }

        private void CursAcademic_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new CursAcademicSetWindow()
            {
                DataContext = new CursAcademicSetViewModel()
            };

            w.ShowDialog(this);
        }

        private void Alumne_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new AlumneCreateWindow(); // <--- caldrà substituir per ALumneSet. ToDo issue20
            w.ShowDialog(this);
        }

        private void GoodbyeButtonMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            SnackbarHost.Post("See ya next time, user!");
        }
    }
}