using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Windows.Web.Http.Filters;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace WTalk.Universal
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
        Windows.Web.Http.HttpCookieManager cookieManager;
        public LoginPage()
        {
            this.InitializeComponent();
            this.Loaded += LoginPage_Loaded;
            login_webview.NavigationCompleted += Login_webview_NavigationCompleted;
            cookieManager = httpBaseProtocolFilter.CookieManager;
        }

        private void Login_webview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.Uri.AbsoluteUri.Contains("as="))
            {                   
                Windows.Web.Http.HttpCookie oauth_code = cookieManager.GetCookies(args.Uri).Cast<Windows.Web.Http.HttpCookie>().FirstOrDefault(c => c.Name == "oauth_code");
                WTalk.AuthenticationManager.Current.AuthenticateWithCode(oauth_code.Value);
                Frame rootFrame = Window.Current.Content as Frame;
                AppShell.Current.AppFrame.Navigate(typeof(Views.MasterDetailPage), typeof(Views.ConversationListPage));                
                return;
            }
        }

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            Uri url = new Uri(WTalk.AuthenticationManager.Current.GetCodeUrl());
            //var cookieCollection = cookieManager.GetCookies(url);
            //foreach (var cookie in cookieCollection)
            //    cookieManager.DeleteCookie(cookie);
            login_webview.Navigate(url);
        }

    }
}
