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
    public class CursAcademicRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        protected dtoo.CursAcademic Model { get; }
        protected ObservableCollectionExtended<CursAcademicRowViewModel> TotsElsCursos {get;}
        public CursAcademicRowViewModel(
            dtoo.CursAcademic CursAcademicDto, 
            ObservableCollectionExtended<CursAcademicRowViewModel> totsElsCursos,
            bool modeLookup = false)
        {

            // Behavior Parm
            ModeLookup = modeLookup;
            TotsElsCursos = totsElsCursos;

            // State
            Model = CursAcademicDto;
            _Etiqueta = CursAcademicDto.Etiqueta;
            _Descripcio = CursAcademicDto.Descripcio;
            _Estat = CursAcademicDto.EsActiu ? "Activat" : "Desactivat";
            _EsActiu = CursAcademicDto.EsActiu;
            Id = CursAcademicDto.Id;

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

        private string _Estat = string.Empty;
        public string Estat
        {
            get { return _Estat; }
            protected set { this.RaiseAndSetIfChanged(ref _Estat, value); }
        }

        private string _Descripcio = string.Empty;
        public string Descripcio
        {
            get { return _Descripcio; }
            protected set { this.RaiseAndSetIfChanged(ref _Descripcio, value); }
        }

        private bool _EsActiu;
        public bool EsActiu
        {
            get { return _EsActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        public int Id { get; }

        private void DTO2ModelView(dtoo.CursAcademic? data)
        {
            if (data == null)
                return;

            Etiqueta = data.Etiqueta;
            Descripcio = data.Descripcio;
            Estat = data.EsActiu ? "Activat" : "Desactivat";
            EsActiu = data.EsActiu;
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
            using var bl = SuperContext.GetBLOperation<ICursAcademicActivaDesactiva>();
            var dto = await bl.Toggle(Id);
            DTO2ModelView(dto.Data);
            BrokenRules2ModelView(dto.BrokenRules);

            if (dto.Data == null)
                return;

            TotsElsCursos
                .Where(x=>x.Id != dto.Data.Id)
                .ToList()
                .ForEach(x=>x.EsActiu = false);
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<CursAcademicUpdateViewModel, dtoo.CursAcademic?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new CursAcademicUpdateViewModel(Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) DTO2ModelView(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, dtoo.CursAcademic> SeleccionarCommand { get; }
        private dtoo.CursAcademic SelectRow() => Model;

    }
}
