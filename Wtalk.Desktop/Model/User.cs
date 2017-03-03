using System.Collections.Generic;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Core.Utils;
using WTalk.Desktop.Mvvm;

namespace WTalk.Desktop.Model
{
    public class User : ObservableObject
    {
        Entity _entity;
        internal User(Entity entity)
        {
            _entity = entity;
        }

        public User() { }

        public string Id { get { return _entity.id.gaia_id; } }
        public string DisplayName { get { return _entity.properties.display_name; } }
        public string FirstName { get { return _entity.properties.first_name; } }
        public string PhotoUrl
        {
            get
            {
                return FileCache.Current.GetOrUpdate(Id, string.Format("https:{0}", _entity.properties.photo_url));
            }
        }
        public List<string> Emails { get { return _entity.properties.email; } }
        public bool Online { get { return _entity.presence != null ? _entity.presence.available : false; } }
        public string Email { get { return _entity.properties.canonical_email; } }

        internal void SetPresence(Presence presence)
        {
            _entity.presence = presence;
            OnPropertyChanged(nameof(Online));
        }

        public bool IsSynced
        {
            get { return this._entity != null; }
        }
    }


}
