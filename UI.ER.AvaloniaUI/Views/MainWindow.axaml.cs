using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Material.Styles.Controls;
using UI.ER.AvaloniaUI.Helpers;
using UI.ER.AvaloniaUI.Pages;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;
using ReactiveUI.Avalonia;
using System;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive;
using CommonInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace UI.ER.AvaloniaUI.Views
{
    // MainWindow és l'única vista fora de convenció: el seu ViewModel no es diu
    // MainViewModel. Es declara aquí en comptes de tractar-la com a cas especial
    // dins de la factory.
    [ViewModel(typeof(AppStatusViewModel))]
    public partial class MainWindow : ReactiveWindow<AppStatusViewModel>
    {
        private readonly IWindowFactory _windows;
        private NavigationDrawer? _leftDrawer;
        private ToggleButton? _navSwitch;

        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen _windows a punt.
        public MainWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public MainWindow(IWindowFactory windows)
        {
            _windows = windows;

            this.WhenActivated(disposables =>
            {
                RegisterShowAlumneDialog(disposables);
                RegisterShowActuacioDialog(disposables);
                RegisterShowCursAcademicDialog(disposables);
            });

            InitializeComponent();
            this.AttachDevTools(KeyGesture.Parse("Shift+F12"));

            // Get controls and set up two-way sync
            _leftDrawer = this.FindControl<NavigationDrawer>("LeftDrawer");
            _navSwitch = this.FindControl<ToggleButton>("NavDrawerSwitch");

            if (_navSwitch != null && _leftDrawer != null)
            {
                // Force closed state on startup
                _leftDrawer.LeftDrawerOpened = false;
                _navSwitch.IsChecked = false;

                // Sync toggle button changes to drawer
                _navSwitch.IsCheckedChanged += (s, e) =>
                {
                    _leftDrawer.LeftDrawerOpened = _navSwitch.IsChecked ?? false;
                };

                // Sync drawer changes back to toggle button
                _leftDrawer.PropertyChanged += (s, e) =>
                {
                    if (e.Property == NavigationDrawer.LeftDrawerOpenedProperty)
                    {
                        _navSwitch.IsChecked = _leftDrawer.LeftDrawerOpened;
                    }
                };
            }
        }

        //
        protected virtual void RegisterShowAlumneDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.ShowAlumneSetDialog.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.Get<AlumneSetWindow>();
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(this.GetOwnerWindow());
                    interaction.SetOutput(result);
                }))
            );

        //
        protected virtual void RegisterShowActuacioDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.ShowActuacioSetDialog.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.Get<ActuacioSetWindow>();
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(this.GetOwnerWindow());
                    interaction.SetOutput(result);
                }))
            );

        //
        protected virtual void RegisterShowCursAcademicDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.ShowCursAcademicSetDialog.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.Get<CursAcademicSetWindow>();
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(this.GetOwnerWindow());
                    interaction.SetOutput(result);
                }))
            );

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            #region Control getter and event binding
            NavDrawerSwitch = this.Get<ToggleButton>(nameof(NavDrawerSwitch));

            DrawerList = this.Get<ListBox>(nameof(DrawerList));
            DrawerList.PointerReleased += DrawerSelectionChanged;
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
            SnackbarHost.Post("EAP Recull et desitja què passis un bon dia :)", "Root", DispatcherPriority.Normal);
        }

        private void Centre_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = _windows.Get<CentreSetWindow>();

            w.ShowDialog(this);
        }

        private void Etapa_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = _windows.Get<EtapaSetWindow>();

            w.ShowDialog(this);
        }

        private void CursAcademic_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = _windows.Get<CursAcademicSetWindow>();

            w.ShowDialog(this);
        }

        private void TipusActuacio_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = _windows.Get<TipusActuacioSetWindow>();

            w.ShowDialog(this);
        }

        private void Alumne_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = _windows.Get<AlumneSetWindow>();

            w.ShowDialog(this);
        }

        private void Actuacio_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = _windows.Get<ActuacioSetWindow>();

            w.ShowDialog(this);
        }


        private void Utilitats_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = _windows.Get<UtilitatsWindow>();

            w.ShowDialog(this);
        }

        private void GoodbyeButtonMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            SnackbarHost.Post("See ya next time, user!", "Root", DispatcherPriority.Normal);
        }


    }
}