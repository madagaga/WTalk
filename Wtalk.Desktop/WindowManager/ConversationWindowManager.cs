using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wtalk.Desktop.ViewModel;
using Wtalk.Desktop.Extension;
using System.Windows.Input;

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
                Window.message_textbox.PreviewKeyDown+=message_textbox_KeyDown;
                Window.DataContext = ViewModel;
            }
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window.Hide();
            e.Cancel = true;
        }

        public void Show(bool bringToFront = false)
        {
            Init();
            Window.ShowActivated = true;
            Window.Show();
            if (bringToFront)
                Window.Activate();
        }

        private void message_textbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            System.Windows.Controls.TextBox box = sender as System.Windows.Controls.TextBox;
            if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.Enter))
            {
                box.Text += Environment.NewLine;
                box.SelectionStart = box.Text.Length;
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.Enter))
            {                
                ((MainViewModel)System.Windows.Application.Current.MainWindow.DataContext).SetPresenceCommand.Execute(null);
                
                ViewModel.SendMessageCommand.Execute(box.Text);
                box.Text = string.Empty;
                e.Handled = true;
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
