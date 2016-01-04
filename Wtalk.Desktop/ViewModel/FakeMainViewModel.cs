using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Wtalk.MvvM;
using WTalk.Model;
using WTalk.Core.ProtoJson.Schema;

namespace Wtalk.Desktop.ViewModel
{
    internal class FakeMainViewModel : ObservableObject
    {
        public ListCollectionView Contacts { get; set; }
        public User CurrentUser { get; private set; }
        public bool AuthenticationRequiered { get { return false; } }
        public FakeMainViewModel()
        {
            Entity entity = new Entity()
            {
                properties = new EntityProperties() { display_name = "Test", canonical_email="test@test.fr"},
                presence = new Presence() {  available = false}
            };
            CurrentUser = new User(entity);
            List<User> _contacts = new List<User>();
            _contacts.Add(CurrentUser);

            Contacts = CollectionViewSource.GetDefaultView(_contacts) as ListCollectionView; 
        }
    }
}
