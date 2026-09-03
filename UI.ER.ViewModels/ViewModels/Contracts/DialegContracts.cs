using System.Collections.Generic;
using System.Reactive;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using ReactiveUI;

namespace UI.ER.ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel d'un diàleg d'edició (alta o modificació) que retorna el DTO desat,
    /// o <c>null</c> si el desat no s'ha arribat a fer.
    /// </summary>
    /// <remarks>
    /// R3 — la declara la vista base <c>EntityEditWindow&lt;TVm, TDto&gt;</c> per poder
    /// tancar-se sola quan el desat va bé. Cap ViewModel canvia de comportament en
    /// implementar-la: només fa explícit un membre que ja tenien tots.
    /// </remarks>
    public interface ISubmitViewModel<TDto> where TDto : class
    {
        ReactiveCommand<Unit, TDto?> SubmitCommand { get; }
    }

    /// <summary>
    /// ViewModel d'una llista que sap demanar el diàleg d'alta de la seva entitat.
    /// </summary>
    public interface ISetViewModel<TCreateVm, TDto>
        where TCreateVm : ViewModelBase
        where TDto : class
    {
        Interaction<TCreateVm, TDto?> ShowDialog { get; }
    }

    /// <summary>
    /// El que una llista necessita saber de les seves files per poder-les refrescar en
    /// silenci: quin Id tenen, quines entitats pinten i com se'ls hi torna a donar el DTO.
    /// </summary>
    /// <remarks>
    /// Separada d'<see cref="IRowViewModel{TUpdateVm,TResultat,TDto}"/> perquè
    /// <c>SetViewModelBase</c> només necessita aquests tres membres i no els ViewModels de
    /// diàleg que l'altra arrossega.
    /// </remarks>
    public interface IFilaDeLlista<TDto> : IId
        where TDto : class
    {
        /// <summary>
        /// Les entitats que la fila té pintades, recalculades a cada
        /// <see cref="Actualitza"/>: una fila que canvia de centre canvia de referències.
        /// </summary>
        IReadOnlySet<Referencia> ReferenciesPintades { get; }

        /// <summary>Torna a pintar la fila amb el DTO que ha arribat de la consulta.</summary>
        void Actualitza(TDto dto);
    }

    /// <summary>
    /// ViewModel d'una fila de llista: sap demanar el diàleg d'edició i sap retornar-se
    /// a si mateix quan la llista treballa en mode lookup.
    /// </summary>
    /// <typeparam name="TUpdateVm">ViewModel del diàleg d'edició.</typeparam>
    /// <typeparam name="TResultat">
    /// Què retorna aquell diàleg. Normalment el mateix DTO, però
    /// <see cref="ActuacioRowViewModel"/> en retorna un <c>EditDialogResult</c> perquè
    /// també pot esborrar.
    /// </typeparam>
    /// <typeparam name="TDto">DTO que la fila retorna en seleccionar-se.</typeparam>
    public interface IRowViewModel<TUpdateVm, TResultat, TDto> : IFilaDeLlista<TDto>
        where TUpdateVm : ViewModelBase
        where TResultat : class
        where TDto : class
    {
        Interaction<TUpdateVm, TResultat?> ShowUpdateDialog { get; }

        ReactiveCommand<Unit, TDto> SeleccionarCommand { get; }
    }
}
