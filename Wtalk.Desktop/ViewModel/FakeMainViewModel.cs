using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wtalk.MvvM;
using WTalk.Model;

namespace Wtalk.Desktop.ViewModel
{
    internal class FakeMainViewModel : ObservableObject
    {
        public List<User> Contacts { get; set; }
        public User CurrentUser { get; private set; }
        public bool AuthenticationRequiered { get { return false; } }
        public FakeMainViewModel()
        {
            WTalk.ProtoJson.Entity entity = new WTalk.ProtoJson.Entity()
            {
                properties = new WTalk.ProtoJson.EntityProperties() { display_name = "Test", canonical_email="test@test.fr"},
                presence = new WTalk.ProtoJson.Presence() {  available = false}
            };
            CurrentUser = new User(entity);
            Contacts = new List<User>();
            Contacts.Add(CurrentUser);
        }
    }
}
