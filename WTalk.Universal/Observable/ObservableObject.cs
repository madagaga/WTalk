using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Universal.Observable
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Windows.UI.Core.CoreDispatcher _dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;


        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                 _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                });
        }

    }
}
