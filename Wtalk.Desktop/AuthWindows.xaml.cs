using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Wtalk.Desktop
{
    /// <summary>
    /// Interaction logic for AuthWindows.xaml
    /// </summary>
    public partial class AuthWindows : Window
    {
        public AuthWindows()
        {
            InitializeComponent();
            this.Loaded += AuthWindows_Loaded;            
            webBrowser.Navigated += webBrowser_Navigated;
            
        }

        void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("approval"))
            {
                webBrowser.Visibility = System.Windows.Visibility.Hidden;
                loading.Visibility = System.Windows.Visibility.Visible;
                
                string title = ((dynamic)webBrowser.Document).Title;
                string code = title.Split('=')[1];
                WTalk.AuthenticationManager.Current.AuthenticateWithCode(code);
                this.Close();
                return;
            }

        }

        void AuthWindows_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser.Source = new Uri(WTalk.AuthenticationManager.Current.GetCodeUrl());
        }
    }
}
