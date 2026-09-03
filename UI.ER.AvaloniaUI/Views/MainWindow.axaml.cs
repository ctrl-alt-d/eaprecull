using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
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
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using ReactiveUI;
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

            this.WhenActivated(Registra);

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

        /// <summary>
        /// Diu quina finestra atén cada petició de navegació del ViewModel. És l'única
        /// cosa que la finestra principal sap de la navegació: quan s'obre i què passa
        /// en tancar-se ho decideix <see cref="AppStatusViewModel"/>.
        /// </summary>
        /// <remarks>
        /// El <see cref="CompositeDisposable"/> de l'activació recull tant la subscripció
        /// exterior com els <c>RegisterHandler</c> que s'hi pengen; sense això
        /// s'acumularien a cada reactivació de la finestra.
        /// </remarks>
        private void Registra(CompositeDisposable d)
            => this
                .WhenAnyValue(x => x.ViewModel)
                .Where(vm => vm is not null)
                .Subscribe(vm =>
                {
                    this.RegistraNavegacio<ActuacioSetWindow>(_windows, vm!.ShowActuacioSetDialog).DisposeWith(d);
                    this.RegistraNavegacio<AlumneSetWindow>(_windows, vm.ShowAlumneSetDialog).DisposeWith(d);
                    this.RegistraNavegacio<CentreSetWindow>(_windows, vm.ShowCentreSetDialog).DisposeWith(d);
                    this.RegistraNavegacio<CursAcademicSetWindow>(_windows, vm.ShowCursAcademicSetDialog).DisposeWith(d);
                    this.RegistraNavegacio<EtapaSetWindow>(_windows, vm.ShowEtapaSetDialog).DisposeWith(d);
                    this.RegistraNavegacio<TipusActuacioSetWindow>(_windows, vm.ShowTipusActuacioSetDialog).DisposeWith(d);
                    this.RegistraNavegacio<UtilitatsWindow>(_windows, vm.ShowUtilitatsDialog).DisposeWith(d);
                })
                .DisposeWith(d);

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
            PageCarousel.SelectedIndex = listBox.SelectedIndex;
            NavDrawerSwitch.IsChecked = false;
        }

        private void TemplatedControl_OnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            SnackbarHost.Post("EAP Recull et desitja què passis un bon dia :)", "Root", DispatcherPriority.Normal);
        }

        private void SortirMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            // Heretat de la plantilla de Material.Avalonia, l'entrada només escrivia
            // un missatge a la snackbar i no tancava res.
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
            else
                Close();
        }

        private void TemaMenuItem_OnClick(object? sender, RoutedEventArgs e) => Tema.Alterna();


    }
}