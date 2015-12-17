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
            this.Closing += MainWindow_Closing;
            this.Activated += MainWindow_Activated;
            
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
                ((MainViewModel)this.DataContext).SetPresenceCommand.Execute(null);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Quit ? ", "WTalk", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                this.WindowState = System.Windows.WindowState.Minimized;
                e.Cancel = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
