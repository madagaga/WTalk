using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WTalk.Mvvm;
using WTalk.Core.ProtoJson.Schema;

namespace WTalk.Model
{
    public class User : ObservableObject
    {
        Entity _entity;
        internal User(Entity entity)
        {
            _entity = entity;
        }

        public User() { }

        public string Id { get { return _entity.id.chat_id; } }
        public string DisplayName { get { return _entity.properties.display_name; } }
        public string FirstName { get { return _entity.properties.first_name; } }
        public string PhotoUrl { get { return string.Format("https:{0}",_entity.properties.photo_url); } }
        public List<string> Emails { get { return _entity.properties.email; } }
        public bool Online { get { return _entity.presence != null ? _entity.presence.available : false; } }
        public string Email { get { return _entity.properties.canonical_email; } }

        internal void SetPresence(Presence presence)
        {
            _entity.presence = presence;
            OnPropertyChanged("Online");
        }
    }

    public class UserSorter : System.Collections.IComparer
    {
        public int Compare(User x, User y)
        {
            return x.DisplayName.CompareTo(y.DisplayName);// +x.Online.CompareTo(y.Online);
        }

        public int Compare(object x, object y)
        {
            return Compare(x as User, y as User);
        }
    }

}
