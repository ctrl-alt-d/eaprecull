using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using dtoi = DTO.i.DTOs;
using UI.ER.ViewModels.Common;
using System.Linq;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{
    public class AlumneUpdateViewModel : ViewModelBase, IId
    {

        public AlumneUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(UpdateData);

        }
        public int Id { get; }
        public string IdTxt => $"Alumne #{Id}";

        public string _Cognoms = string.Empty;
        public string Cognoms
        {
            get => _Cognoms;
            set
            {
                // if (string.IsNullOrWhiteSpace(value))
                // {
                //     throw new NotifyDataErrorInfo("Aquest camp no pot quedat buit.");
                // }

                this.RaiseAndSetIfChanged(ref _Cognoms, value);
            }
        }
        public string _Nom = string.Empty;
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
            using var bl = SuperContext.GetBLOperation<IAlumneSet>();
            var dto = await bl.FromId(Id);

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));
            DTO2ModelView(dto.Data);
        }

        private void DTO2ModelView(dtoo.Alumne? data)
        {
            if (data == null) return;

            Cognoms = data.Cognoms;
            Nom = data.Nom;
            EsActiu = data.EsActiu;
        }

        public virtual async Task<dtoo.Alumne?> UpdateData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.AlumneUpdateParms(
                Id,
                Nom,
                Cognoms, null, null, 
                1,
                null,
                null,
                "",
                null,
                "",
                "",
                EsActiu);

            // cridar backend
            using var bl = SuperContext.GetBLOperation<IAlumneUpdate>();
            var dto = await bl.Update(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            //
            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Alumne?> SubmitCommand { get; }


    }
}