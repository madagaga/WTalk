using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WTalk.Universal.ViewModel;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace WTalk.Universal.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MasterDetailPage : Page
    {
        private MainViewModel ViewModel => AppShell.Current.ViewModel;

        Frame detailFrame;

        public MasterDetailPage()
        {
            this.InitializeComponent();
            this.DataContext = ViewModel;
            MasterFrame.Navigate(typeof(ConversationListPage));
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            detailFrame = this.AdaptiveStates.CurrentState == NarrowState ? MasterFrame : DetailFrame;

        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.MasterFrame.BackStack.Clear();
            this.DetailFrame.BackStack.Clear();


            switch (e.PropertyName)
            {
                case nameof(ViewModel.SelectedConversation):
                    if(detailFrame.CurrentSourcePageType != typeof(ConversationPage))
                        detailFrame.Navigate(typeof(ConversationPage));
                    break;
                case nameof(ViewModel.SelectedContact):
                    break;

                default:
                    break;
            }
                        
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(e.Parameter is Type)
            {
                var viewType = e.Parameter as Type;
                MasterFrame.Navigate(viewType);
                if (AdaptiveStates.CurrentState == DefaultState)
                    DetailFrame.Navigate(typeof(ConversationPage));
            }
        }

        private void AdaptiveStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState == NarrowState)
            {
                detailFrame = MasterFrame;
                DetailFrame.Content = null;
            }
            else
                detailFrame = DetailFrame;
        }
    }
}
