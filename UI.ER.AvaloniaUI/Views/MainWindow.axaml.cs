using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Material.Styles.Controls;
using UI.ER.AvaloniaUI.Pages;
using UI.ER.ViewModels.ViewModels;
using Avalonia.ReactiveUI;
using System;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive;
using CommonInterfaces;

namespace UI.ER.AvaloniaUI.Views
{
    public partial class MainWindow : ReactiveWindow<AppStatusViewModel>
    {
        public MainWindow()
        {

            DataContext = new AppStatusViewModel();

            this.WhenActivated(disposables =>
            {
                RegisterShowAlumneDialog(disposables);
                RegisterShowActuacioDialog(disposables);
                RegisterShowCursAcademicDialog(disposables);
            });

            InitializeComponent();
            this.AttachDevTools(KeyGesture.Parse("Shift+F12"));

        }

        //
        protected virtual void RegisterShowAlumneDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.ShowAlumneSetDialog.RegisterHandler(async interaction =>
                {
                    var dialog = new AlumneSetWindow()
                    {
                        DataContext = new AlumneSetViewModel(modeLookup: false)
                    };
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(GetWindow());
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
                    var dialog = new ActuacioSetWindow()
                    {
                        DataContext = new ActuacioSetViewModel(modeLookup: false)
                    };
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(GetWindow());
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
                    var dialog = new CursAcademicSetWindow()
                    {
                        DataContext = new CursAcademicSetViewModel(modeLookup: false)
                    };
                    var result = await dialog.ShowDialog<IIdEtiquetaDescripcio?>(GetWindow());
                    interaction.SetOutput(result);
                }))
            );

        private Window GetWindow()
            =>
            (Window)this.VisualRoot!;

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

        private void TipusActuacio_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new TipusActuacioSetWindow()
            {
                DataContext = new TipusActuacioSetViewModel()
            };

            w.ShowDialog(this);
        }

        private void Alumne_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new AlumneSetWindow()
            {
                DataContext = new AlumneSetViewModel()
            };

            w.ShowDialog(this);
        }

        private void Actuacio_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new ActuacioSetWindow()
            {
                DataContext = new ActuacioSetViewModel()
            };

            w.ShowDialog(this);
        }


        private void Utilitats_OnClick(object? sender, RoutedEventArgs e)
        {
            var w = new UtilitatsWindow()
            {
                DataContext = new UtilitatsViewModel()
            };

            w.ShowDialog(this);            
        }

        private void GoodbyeButtonMenuItem_OnClick(object? sender, RoutedEventArgs e)
        {
            SnackbarHost.Post("See ya next time, user!", "Root", DispatcherPriority.Normal);
        }
    }
}