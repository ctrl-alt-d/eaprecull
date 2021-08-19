﻿using System.Linq;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using dtoi = DTO.i.DTOs;
using UI.ER.AvaloniaUI.Services;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{

    public class AlumneSetViewModel : ViewModelBase
    {
        public bool ModeLookup { get; }
        public AlumneSetViewModel(bool modeLookup = false)
        {

            ModeLookup = modeLookup;

            // Filtre
            var NomCognomsCentreObserver =
                this
                .WhenAnyValue(x=>x.NomCognomsTagCentre)
                .Throttle(TimeSpan.FromMilliseconds(400));

            var OrdreAlfabeticObserver =
                this
                .WhenAnyValue(x=>x.OrdreAlfabetic);

            this
                .WhenAnyValue(x => x.NomesActius)
                .CombineLatest(
                        NomCognomsCentreObserver,
                        OrdreAlfabeticObserver,
                        (nomesActius, NomCognomsTagCentre, ordreAlfabetic) => 
                        (nomesActius, NomCognomsTagCentre, ordreAlfabetic)
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t => LoadAlumnes(t.nomesActius, t.NomCognomsTagCentre, t.ordreAlfabetic))
                ;

            // Create
            ShowDialog = new Interaction<AlumneCreateViewModel, dtoo.Alumne?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new AlumneCreateViewModel();
                var data = await ShowDialog.Handle(update);
                var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new dtoi.EsActiuParms(true));
                var cursActual = cursActual_dto.Data?.FirstOrDefault();

                if (data != null)
                {
                    var item = new AlumneRowViewModel(data, cursActual, ModeLookup);
                    MyItems.Insert(0, item);
                }
            });


        }
        public ObservableCollectionExtended<AlumneRowViewModel> MyItems { get; } = new();

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        protected virtual async void LoadAlumnes(bool nomesActius, string NomCognomsTagCentre, bool ordreAlfabetic)
        {
            Loading = true;
            MyItems.Clear();
            await OmplirAmbElsNousValors(nomesActius, NomCognomsTagCentre, ordreAlfabetic);
        }

        private async Task OmplirAmbElsNousValors(bool nomesActius, string NomCognomsTagCentre, bool ordreAlfabetic)
        {
            // Preparar paràmetres al backend
            var esActiu = 
                nomesActius ? 
                true : 
                (bool?)null;
                
            var ordre = 
                ordreAlfabetic ?
                dtoi.AlumneSearchParms.OrdreResultatsChoice.CognomsNom :
                dtoi.AlumneSearchParms.OrdreResultatsChoice.DarreraModificacio ;

            var parms = new DTO.i.DTOs.AlumneSearchParms(
                esActiu: esActiu,
                nomCognomsTagCentre: NomCognomsTagCentre,
                ordreResultats: ordre
            );

            // Petició al backend            
            using var bl = SuperContext.GetBLOperation<IAlumneSet>();
            var dto = await bl.FromPredicate(parms);

            // 
            BrokenRules.Clear();
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));


            // Ha fallat la petició
            if (dto.Data == null)
                throw new Exception("Error en fer petició al backend"); // ToDo: gestionar broken rules            

            //
            var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new dtoi.EsActiuParms(true));
            var cursActual = cursActual_dto.Data?.FirstOrDefault();

            // Tenim els resultats
            var newItems =
                dto
                .Data
                .Select(x => new AlumneRowViewModel(x, cursActual, ModeLookup));

            MyItems.AddRange(newItems);

            //
            PaginatedMsg =
                (dto.Total > dto.TakeRequested) ?
                $"Mostrant els {newItems.Count()} primers resultats d'un total de {dto.Total}" :
                string.Empty;

            Loading = false;
        }

        // Warning
        private string _PaginatedMsg = string.Empty;
        public string PaginatedMsg
        {
            get => _PaginatedMsg;
            set => this.RaiseAndSetIfChanged(ref _PaginatedMsg, value);
        }

        // Filtre
        private bool _Loading = true;
        public bool Loading
        {
            get => _Loading;
            set => this.RaiseAndSetIfChanged(ref _Loading, value);
        }

        private bool _NomesActius = true;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }

        private bool _OrdreAlfabetic = false;
        public bool OrdreAlfabetic
        {
            get => _OrdreAlfabetic;
            set => this.RaiseAndSetIfChanged(ref _OrdreAlfabetic, value);
        }

        private string _NomCognomsCentre = string.Empty;
        public string NomCognomsTagCentre
        {
            get => _NomCognomsCentre;
            set => this.RaiseAndSetIfChanged(ref _NomCognomsCentre, value);
        }

        // Crear item
        public ICommand Create { get; }
        public Interaction<AlumneCreateViewModel, dtoo.Alumne?> ShowDialog { get; }


    }
}
