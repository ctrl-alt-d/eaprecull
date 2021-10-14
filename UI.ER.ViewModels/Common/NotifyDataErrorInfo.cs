using System;
using System.Collections;
using System.ComponentModel;

namespace UI.ER.ViewModels.Common
{
    public class NotifyDataErrorInfo : Exception, INotifyDataErrorInfo
    {
        public NotifyDataErrorInfo(object errorData)
            : base(errorData?.ToString())
        {
            ErrorData = errorData!;
            _ = ErrorsChanged;
        }

        public object ErrorData { get; }
        public bool HasErrors => true;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public IEnumerable GetErrors(string? propertyName)
        {
            return new[] { ErrorData.ToString() };
        }

        public override string ToString()
        {
            return ErrorData.ToString() ?? "Error a les dades";
        }
    }
}