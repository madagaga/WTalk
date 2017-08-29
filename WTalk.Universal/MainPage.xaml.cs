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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WTalk.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel Model { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = Model = new MainViewModel();
                        
            VisualGroups.CurrentStateChanging += (s, e) => {
                if (e.NewState == Small)
                {
                    Model.SelectedConversation = null;
                    conversationLargeView.Visibility = Visibility.Collapsed;
                }
                if (masterView.SelectedIndex == 2)
                    masterView.SelectedIndex = 0;
            };

        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (VisualGroups.CurrentState == Small)
            {
                conversationSmallView.IsSelected = true;
            }
        }                
    }
}
