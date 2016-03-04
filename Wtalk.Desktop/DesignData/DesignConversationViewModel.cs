using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wtalk.Desktop.CustomDataTemplate;
using WTalk.Mvvm;

namespace Wtalk.Desktop.DesignData
{
    internal class DesignConversationViewModel : ObservableObject
    {
        
        public string Participants { get { return "Test"; } }
        public string CurrentUserId { get { return "1"; } }
        public List<dynamic> Messages { get; private set; }
        public bool HistoryEnabled { get { return true; } }
        public DesignConversationViewModel()
        {
                        
            Messages = new List<dynamic>();
            Messages.Add(
                new
                {
                    SenderId = "2",
                    Content = "Message 1 ",
                    MessageDateTime = DateTime.Now
                }
                );

            Messages.Add(
                new
                {
                    SenderId = "1",
                    Content = "Message 2 ",
                    MessageDateTime = DateTime.Now
                }
                );
        }
    }
}
