﻿using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using UI.ER.ViewModels.Common;
using System.Linq;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{
    public class AlumneRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        protected dtoo.Alumne Model { get; set;}
        protected readonly dtoo.CursAcademic? CursActual;
        public AlumneRowViewModel(dtoo.Alumne data, dtoo.CursAcademic? cursActual, bool modeLookup = false)
        {

            // Behavior Parm
            ModeLookup = modeLookup;
            Id = data.Id;
            Model = data;
            CursActual = cursActual;

            // State
            DTO2ModelView(data);

            // Behavior
            DoActiuToggleCommand = ReactiveCommand.CreateFromTask(RunActiuToggle);
            SeleccionarCommand = ReactiveCommand.Create(SelectRow);
            UpdateCommand = ReactiveCommand.CreateFromTask(ShowUpdateDialogHandle);
        }


        public bool ModeLookup { get; }

        private string _Etiqueta = string.Empty;
        public string Etiqueta
        {
            get { return _Etiqueta; }
            protected set { this.RaiseAndSetIfChanged(ref _Etiqueta, value); }
        }

        private bool _Desactualitzat;
        public bool Desactualitzat
        {
            get { return _Desactualitzat; }
            protected set { this.RaiseAndSetIfChanged(ref _Desactualitzat, value); }
        }

        private string _Descripcio = string.Empty;
        public string Descripcio
        {
            get { return _Descripcio; }
            protected set { this.RaiseAndSetIfChanged(ref _Descripcio, value); }
        }

        private string _CentreActual = string.Empty;
        public string CentreActual
        {
            get { return _CentreActual; }
            protected set { this.RaiseAndSetIfChanged(ref _CentreActual, value); }
        }

        private string _CursDarreraActualitzacio = string.Empty;
        public string CursDarreraActualitzacio
        {
            get { return _CursDarreraActualitzacio; }
            protected set { this.RaiseAndSetIfChanged(ref _CursDarreraActualitzacio, value); }
        }

        private bool _EsActiu;
        public bool EsActiu
        {
            get { return _EsActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        public int Id { get; }

        private void DTO2ModelView(dtoo.Alumne? AlumneDto)
        {
            if (AlumneDto == null)
                return;

            Model = AlumneDto;
            Etiqueta = AlumneDto.Etiqueta;
            Descripcio = AlumneDto.Descripcio;
            CentreActual = AlumneDto.CentreActual?.Etiqueta ?? "** Sense centre assignat **";
            Desactualitzat = CursActual != null && AlumneDto.CursDarreraActualitacioDades.Id != CursActual.Id;
            EsActiu = AlumneDto.EsActiu;
            CursDarreraActualitzacio = $"Curs darrera actualització de dades: {AlumneDto.CursDarreraActualitacioDades.Descripcio}";
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }

        // --- Activar / Desactivar ---
        public ReactiveCommand<Unit, Unit> DoActiuToggleCommand { get; }
        protected async Task RunActiuToggle()
        {
            using var bl = SuperContext.GetBLOperation<IAlumneActivaDesactiva>();
            var dto = await bl.Toggle(Id);
            DTO2ModelView(dto.Data);
            BrokenRules2ModelView(dto.BrokenRules);
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<AlumneUpdateViewModel, dtoo.Alumne?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new AlumneUpdateViewModel(Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) DTO2ModelView(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, dtoo.Alumne> SeleccionarCommand { get; }
        private dtoo.Alumne SelectRow() => Model;

    }
}