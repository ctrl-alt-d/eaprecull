using System.Reactive;
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
    public interface IRowViewModel<TUpdateVm, TResultat, TDto>
        where TUpdateVm : ViewModelBase
        where TResultat : class
        where TDto : class
    {
        Interaction<TUpdateVm, TResultat?> ShowUpdateDialog { get; }

        ReactiveCommand<Unit, TDto> SeleccionarCommand { get; }
    }
}
