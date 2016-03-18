using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WTalk.Mvvm;

namespace Wtalk.Desktop.DesignData
{
    internal class DesignMainViewModel : ObservableObject
    {
        public Dictionary<string, dynamic> ActiveContacts { get; set; }
        
        public dynamic CurrentUser { get; private set; }
        public bool AuthenticationRequired { get { return false; } }
        public dynamic SelectedConversation { get; set; }


        public DesignMainViewModel()
        {
            CurrentUser = new
            {
                DisplayName = "User DisplayName"
            };

            ActiveContacts = new Dictionary<string, dynamic>();
            ActiveContacts.Add("1", new
            {
                Contact = new { 
                DisplayName = "User1",
                Email = "test@test.fr",
                Online = true
                },
                LastMessage = "Test message"
            });

            SelectedConversation = new DesignConversationViewModel();
        }
    }
}
