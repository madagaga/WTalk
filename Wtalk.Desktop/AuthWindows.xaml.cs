using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            if (e.Uri.AbsoluteUri.Contains("auth_user"))
            {

                webBrowser.Visibility = System.Windows.Visibility.Hidden;
                
                
                
                this.Close();
                return;
            }

        }

        void AuthWindows_Loaded(object sender, RoutedEventArgs e)
        {               
            webBrowser.LoadCompleted += webBrowser_LoadCompleted;
            Uri url = new Uri(WTalk.AuthenticationManager.Current.GetCodeUrl());
            NativeMethods.SuppressCookiePersistence();
            
            webBrowser.Navigate(url);
            
        }

        void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if(e.Uri.AbsoluteUri.Contains("authuser=0"))
            {
                webBrowser.Visibility = System.Windows.Visibility.Hidden;
                System.Net.CookieContainer container = NativeMethods.GetUriCookieContainer(webBrowser.Source);
                Cookie oauth_code = container.GetCookies(webBrowser.Source).Cast<Cookie>().FirstOrDefault(c => c.Name == "oauth_code");
                WTalk.AuthenticationManager.Current.AuthenticateWithCode(oauth_code.Value);
//                WTalk.AuthenticationManager.Current.RetrieveCode(container, webBrowser.Source.AbsoluteUri);
                this.Close();
                return;
            }
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

        [System.Runtime.InteropServices.DllImport("wininet.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            Int32 dwFlags,
            IntPtr lpReserved);

        private const Int32 InternetCookieHttponly = 0x1000;

        /// <summary>
        /// Gets the URI cookie container.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static System.Net.CookieContainer GetUriCookieContainer(Uri uri)
        {
            System.Net.CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                    null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new System.Net.CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }
    }
}
