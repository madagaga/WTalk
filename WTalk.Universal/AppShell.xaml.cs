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
using WTalk.Universal.Views;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace WTalk.Universal
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class AppShell : Page
    {
        public static AppShell Current = null;

        public MainViewModel ViewModel { get; private set; }

        public Frame AppFrame => AppShellFrame;

        public AppShell()
        {
            this.InitializeComponent();
            Current = this;            
        }

        public void InitializeViewModel()
        {
             ViewModel = new MainViewModel();
        }

        private void sideMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(sideMenu.SelectedIndex)
            {
                case 0:
                    AppFrame.Navigate(typeof(MasterDetailPage), typeof(ConversationListPage));
                    break;
                case 1:
                    AppFrame.Navigate(typeof(MasterDetailPage), typeof(ContactListPage));
                    break;
                default:
                    break;
            }
        }
    }
}
