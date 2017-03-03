using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk;
using WTalk.Desktop.Model;

namespace Wtalk.Desktop
{
    public class Singleton
    {
        static Client _clientInstance;
        public static Client DefaultClient
        {
            get
            {
                if (_clientInstance == null)
                    _clientInstance = new Client();
                return _clientInstance;
            }
        }

        static Dictionary<string, User> _contacts = new Dictionary<string, User>();
        public static User GetUser(string id)
        {
            if (!_contacts.ContainsKey(id))
            {
                var entity = DefaultClient.GetContactFromCache(id).Result;
                _contacts.Add(entity.id.gaia_id, new User(entity));
            }
            return _contacts[id];
        }
    }
}
