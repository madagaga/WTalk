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
            this.Activated += MainWindow_Activated;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            
        }

        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Normal)
                ((MainViewModel)this.DataContext).SetPresenceCommand.Execute(null);
        }        
    }
}
