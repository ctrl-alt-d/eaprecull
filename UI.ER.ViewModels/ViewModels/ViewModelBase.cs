using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;

namespace UI.ER.ViewModels.ViewModels
{
    public class ViewModelBase : ReactiveObject, IValidatableViewModel
    {
        public ValidationContext ValidationContext { get; } = new ValidationContext();
    }
}
