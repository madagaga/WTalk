using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WTalk.Core.Utils;

namespace WTalk
{
    public class AuthenticationManager
    {
        const string CLIENT_ID = "936475272427.apps.googleusercontent.com";
        const string CLIENT_SECRET = "KWsJlkaMn1jGLxQpWxMnOox-";
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

        const string OAUTH_CODE_INIT_URL = "https://accounts.google.com/o/oauth2/programmatic_auth?";
        const string OAUTH_VALIDATION_URL = "https://www.googleapis.com/oauth2/v4/token";
                
        //string[] scopes = new string[] { "https://www.googleapis.com/auth/chat", "https://www.googleapis.com/auth/client_channel", "https://www.googleapis.com/auth/googlevoice", "https://www.googleapis.com/auth/hangouts", "https://www.googleapis.com/auth/photos", "https://www.googleapis.com/auth/plus.circles.read", "https://www.googleapis.com/auth/plus.contactphotos", "https://www.googleapis.com/auth/plus.me", "https://www.googleapis.com/auth/plus.peopleapi.readwrite", "https://www.googleapis.com/auth/youtube.readonly" };
        public readonly string[] Scopes = new string[] { "https://www.google.com/accounts/OAuthLogin", "https://www.googleapis.com/auth/userinfo.email" };

        HttpClient _client;

        // not a good idea ...
        AccessToken _token;

        // file
        string _file;

        NLog.Logger _logger = NLog.LogManager.GetLogger("AuthenticationManager");

        public bool IsAuthenticated { get; private set; }

        // singleton 
        static AuthenticationManager _current;
        public static AuthenticationManager Current
        {
            get
            {
                if (_current == null)
                    _current = new AuthenticationManager();
                return _current;
            }
        }


        
        private AuthenticationManager()
        {            
            _client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false, CookieContainer = Client.CookieContainer, UseCookies = true});
            try
            {
                _file = System.IO.Path.Combine(FileCache.Current.Root, "token.txt");
                if (System.IO.File.Exists(_file))
                    LoadToken();
            }
            catch { }
        }

        #region load / save 

        void LoadToken()
        {
            string content = System.IO.File.ReadAllText(_file);
            if (!string.IsNullOrEmpty(content))
            {
                content = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content));
                _token = coreJson.JSON.Deserialize<AccessToken>(content);
            }
        }

        void SaveToken(string token)
        {
            string content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
            System.IO.File.WriteAllText(_file, content);                        
        }

        #endregion

        #region url generation
        
        public string GetCodeUrl()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(OAUTH_CODE_INIT_URL);
            builder.AppendFormat("client_id={0}", CLIENT_ID);
            builder.AppendFormat("&scope={0}", string.Join("+", Scopes));
            builder.AppendFormat("&hl={0}", "en");
            builder.Append("&access_type=offline");
            builder.Append("&top_level_cookie=1");
            return builder.ToString();
        }

        #endregion
       

        public void AuthenticateWithCode(string code)
        {
            FormUrlEncodedContent c = new FormUrlEncodedContent(
               new[] {
                    new KeyValuePair<string, string>("client_id", CLIENT_ID),
                    new KeyValuePair<string, string>("client_secret", CLIENT_SECRET),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", REDIRECT_URI),
                    }
               );
            var response = _client.PostAsync(OAUTH_VALIDATION_URL, c).Result;
            string content = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                _token = coreJson.JSON.Deserialize<AccessToken>(content);
                SaveToken(content);
                Connect();
            }
        }

        public void Disconnect()
        {
            try
            {
                if(System.IO.File.Exists(_file))
                    System.IO.File.Delete(_file);

                FileCache.Current.Reset();
            }
            catch{}
        }

        public void Connect(AccessToken token = null)
        {
            if (token != null)
                _token = token;

            // if token exists get session cookie;
            if (_token != null && _token.access_token != null)
            {
                if (_client.DefaultRequestHeaders.Contains("Authorization"))
                    _client.DefaultRequestHeaders.Remove("Authorization");

                _client.DefaultRequestHeaders.Add("Authorization", string.Format("{0} {1}", _token.token_type, _token.access_token));
                HttpResponseMessage response = _client.GetAsync("https://www.google.com/accounts/OAuthLogin?source=wtalk&issueuberauth=1").Result;
                if (response.IsSuccessStatusCode)
                {
                    string uberAuth = response.Content.ReadAsStringAsync().Result;
                    response = _client.GetAsync(string.Format("https://accounts.google.com/MergeSession?service=mail&continue=http://www.google.com&uberauth={0}", uberAuth)).Result;

                    // hack because UAP dose not set cookie when redirect
                    while (response.StatusCode == HttpStatusCode.Redirect)
                        response = _client.GetAsync(response.Headers.Location).Result;
                    
                    if (response.IsSuccessStatusCode)
                        this.IsAuthenticated = true;

                }
                else
                {
                    AuthWithRefreshToken();
                    Connect();
                }

            }
        }

        void AuthWithRefreshToken()
        {
            if (_token.refresh_token == null)
            {
                _token = null;
                if (_client.DefaultRequestHeaders.Contains("Authorization"))
                    _client.DefaultRequestHeaders.Remove("Authorization");
                return;
            }
            FormUrlEncodedContent c = new FormUrlEncodedContent(
             new[] { 
                    new KeyValuePair<string, string>("client_id", CLIENT_ID),
                    new KeyValuePair<string, string>("client_secret", CLIENT_SECRET),                    
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token",_token.refresh_token),
                    }
             );

            var response = _client.PostAsync(OAUTH_VALIDATION_URL, c).Result;
            string content = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                _token = coreJson.JSON.Deserialize<AccessToken>(content);
                SaveToken(content);
            }
        }


    }

    public class AccessToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }

}
