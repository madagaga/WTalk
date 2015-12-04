using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wtalk.Desktop.ViewModel;
using Wtalk.Desktop.Extension;

namespace Wtalk.Desktop.WindowManager
{
    public class ConversationWindowManager
    {
        public ConversationWindow Window { get; set; }
        public ConversationViewModel ViewModel { get; set; }
        public ConversationWindowManager(ConversationViewModel model)
        {
            this.ViewModel = model;
            this.ViewModel.AttentionRequired += (s, e) =>
            {
                Init();
                if (!Window.IsVisible)
                {
                    Window.WindowState = System.Windows.WindowState.Minimized;
                    Show();
                }
                Window.FlashWindow();
            };
        }

        void Init()
        {
            if (Window == null)
            {
                Window = new ConversationWindow();
                Window.Activated+=Window_Activated;
                Window.Closing += Window_Closing;
                Window.message_textbox.KeyDown+=message_textbox_KeyDown;
                Window.DataContext = ViewModel;
            }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window.Hide();
            e.Cancel = true;
        }

        public void Show()
        {
            Init();
            Window.ShowActivated = true;
            Window.Show();            
        }

        private void message_textbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                System.Windows.Controls.TextBox box = sender as System.Windows.Controls.TextBox;
                ViewModel.SendMessageCommand.Execute(box.Text);
                box.Text = string.Empty;

            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (Window.WindowState == System.Windows.WindowState.Normal)
            {
                Window.scrollBar.ScrollToBottom();
                ViewModel.SetFocusCommand.Execute(null);
                Window.StopFlashingWindow();
            }
        }
    }
}
