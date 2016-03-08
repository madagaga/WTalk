using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wtalk.MvvM;
using WTalk.Model;

namespace Wtalk.Desktop.ViewModel
{
    internal class FakeMainViewModel : ObservableObject
    {
        public ListCollectionView Contacts { get; set; }
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
            List<User> _contacts = new List<User>();
            _contacts.Add(CurrentUser);

            Contacts = CollectionViewSource.GetDefaultView(_contacts) as ListCollectionView; 
        }
    }
}
