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
            
            
        }

        void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("approval"))
            {
                webBrowser.Visibility = System.Windows.Visibility.Hidden;
                
                
                string title = ((dynamic)webBrowser.Document).Title;
                string code = title.Split('=')[1];
                WTalk.AuthenticationManager.Current.AuthenticateWithCode(code);
                this.Close();
                return;
            }

        }

        void AuthWindows_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser.Navigated += webBrowser_Navigated;
            Uri url = new Uri(WTalk.AuthenticationManager.Current.GetCodeUrl());
            NativeMethods.SuppressCookiePersistence();
            webBrowser.Navigate(url);
            
        }


    }

    public static partial class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("wininet.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private const int INTERNET_OPTION_SUPPRESS_BEHAVIOR = 81;
        private const int INTERNET_SUPPRESS_COOKIE_PERSIST = 3;

        public static void SuppressCookiePersistence()
        {
            var lpBuffer = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(System.Runtime.InteropServices.Marshal.SizeOf(typeof(int)));
            System.Runtime.InteropServices.Marshal.StructureToPtr(INTERNET_SUPPRESS_COOKIE_PERSIST, lpBuffer, true);

            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SUPPRESS_BEHAVIOR, lpBuffer, sizeof(int));

            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(lpBuffer);
        }
    }
}
