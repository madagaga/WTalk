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
        public List<dynamic> ActiveContacts { get; set; }
        
        public dynamic CurrentUser { get; private set; }
        public bool AuthenticationRequired { get { return false; } }
        public dynamic SelectedConversation { get; set; }
        public bool Connected { get { return true; } }

        public DesignMainViewModel()
        {
            CurrentUser = new
            {
                DisplayName = "User DisplayName"
            };

            ActiveContacts = new List<dynamic>();

            ActiveContacts.Add(new
            {
                Contact = new
                {
                    DisplayName = "User1dsdddddddddddddddddds",
                    Email = "test@test.fr",
                    Online = true
                },
                Conversation = new DesignConversationViewModel() { HasUnreadMessages = true}
            });


            for (int i = 10; i < 20;i++ )
                ActiveContacts.Add( new
                {
                    Contact = new
                    {
                        DisplayName = "User1dsdddddddddddddddddds",
                        Email = "test@test.fr",
                        Online = true
                    },
                    Conversation = new DesignConversationViewModel()
                });

            //SelectedConversation = new DesignConversationViewModel();
        }
    }
}
