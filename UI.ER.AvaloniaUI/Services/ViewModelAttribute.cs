using System;

namespace UI.ER.AvaloniaUI.Services
{
    /// <summary>
    /// Declara de forma explícita el ViewModel d'una vista quan aquesta no segueix
    /// la convenció de noms (treure el sufix «Window» o «UserCtrl» i afegir «ViewModel»).
    /// Té prioritat sobre la convenció.
    /// </summary>
    /// <example>
    /// <code>
    /// [ViewModel(typeof(AppStatusViewModel))]
    /// public partial class MainWindow : ReactiveWindow&lt;AppStatusViewModel&gt; { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ViewModelAttribute : Attribute
    {
        public ViewModelAttribute(Type viewModelType) => ViewModelType = viewModelType;

        public Type ViewModelType { get; }
    }
}
