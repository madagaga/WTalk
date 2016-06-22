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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wtalk.Desktop.ViewModel;
using Wtalk.Desktop.Extension;

namespace Wtalk.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();            
            this.Activated += MainWindow_Activated;
            this.Closing += MainWindow_Closing;
            this.sendMessageTextBox.GotFocus += (s, e) => { ((MainViewModel)this.DataContext).SelectedConversation.SetFocusCommand.Execute(null); };
            this.sendMessageTextBox.PreviewKeyDown += (s, e) => { 
                if(e.Key == Key.Enter)
                {
                    e.Handled = true;                    
                    ((MainViewModel)this.DataContext).SelectedConversation.SendMessageCommand.Execute(null);

                }
                else
                    ((MainViewModel)this.DataContext).SelectedConversation.SetUserTypingCommand.Execute(null);

            };

            // scrollViewer scroll position can not be bound
            activeContactList.MouseLeftButtonUp += (s, e) => {
                if (activeContactList.SelectedItem != null)
                    messagesScrollViewer.ScrollToVerticalOffset(((ConversationViewModel)activeContactList.SelectedItem).ScrollPosition);
            };

            messagesScrollViewer.ScrollChanged += (s, e) =>
            {
                if (activeContactList.SelectedItem != null && e.VerticalChange != 0)
                    ((ConversationViewModel)activeContactList.SelectedItem).ScrollPosition = messagesScrollViewer.VerticalOffset;
            };

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (((MainViewModel)this.DataContext).Connected)
                ((MainViewModel)this.DataContext).CurrentPresenceIndex = 3;
        }

        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
            {
                ((MainViewModel)this.DataContext).SetPresenceCommand.Execute(null);
                this.StopFlashingWindow();                
            }
        }        
    }
}
