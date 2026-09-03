using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using Dtoi = DTO.i.DTOs;
using System.Linq;
using DynamicData.Binding;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.ViewModels.ViewModels
{
    public class CentreUpdateViewModel : ViewModelBase, ISubmitViewModel<Dtoo.Centre>, IId
    {

        private readonly IServiceFactory _serveis;

        public CentreUpdateViewModel(IServiceFactory serveis, int id)
        {
            _serveis = serveis;
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(UpdateData);

        }
        public int Id { get; }
        public string IdTxt => $"Centre #{Id}";

        private string _Codi = string.Empty;
        public string Codi
        {
            get => _Codi;
            set
            {
                // if (string.IsNullOrWhiteSpace(value))
                // {
                //     throw new NotifyDataErrorInfo("Aquest camp no pot quedat buit.");
                // }

                this.RaiseAndSetIfChanged(ref _Codi, value);
            }
        }
        private string _Nom = string.Empty;
        public string Nom
        {
            get => _Nom;
            set => this.RaiseAndSetIfChanged(ref _Nom, value);
        }

        private bool _EsActiu;
        public bool EsActiu
        {
            get { return _EsActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        protected virtual async void LoadData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // Backend request
            using var bl = _serveis.GetBLOperation<ICentreSet>();
            var dto = await bl.FromId(Id);

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));
            DTO2ModelView(dto.Data);
        }

        private void DTO2ModelView(Dtoo.Centre? data)
        {
            if (data == null) return;

            Codi = data.Codi;
            Nom = data.Nom;
            EsActiu = data.EsActiu;
        }

        public virtual async Task<Dtoo.Centre?> UpdateData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // preparar paràmetres
            var Parms = new Dtoi.CentreUpdateParms(Id, Codi, Nom, EsActiu);

            // cridar backend
            using var bl = _serveis.GetBLOperation<ICentreUpdate>();
            var dto = await bl.Update(Parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            //
            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, Dtoo.Centre?> SubmitCommand { get; }


    }
}