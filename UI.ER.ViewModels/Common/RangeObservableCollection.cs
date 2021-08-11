using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace UI.ER.ViewModels.Common
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IEnumerable<T> list)
        {
            _suppressNotification = true;
            foreach (T item in list)
            {
                Add(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ClearSilently()
        {
            _suppressNotification = true;
            Clear();
            _suppressNotification = false;
        }
    }
}
