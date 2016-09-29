using Newtonsoft.Json;
using PCLStorage;
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


        IFile _file;
        public AuthenticationManager()
        {
            FileCache.Initialize("cache");
            _client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = Client.CookieContainer, UseCookies = true });
            try
            {
                _file = FileSystem.Current.LocalStorage.CreateFileAsync("token.txt", CreationCollisionOption.OpenIfExists).Result;
                LoadToken();
            }
            catch { }
        }

        void LoadToken()
        {

            string content = _file.ReadAllTextAsync().Result;
            if(!string.IsNullOrEmpty(content))
                _token = JsonConvert.DeserializeObject<AccessToken>(content);
        }

        void SaveToken(string token)
        {
            _file.WriteAllTextAsync(token).Wait();                
        }

        public string GetCodeUrl()
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append(OAUTH_CODE_INIT_URL);
            builder.AppendFormat("client_id={0}", CLIENT_ID);            
            builder.AppendFormat("&scope={0}", string.Join("+", Scopes));
            //builder.AppendFormat("&redirect_uri={0}", REDIRECT_URI);
            //builder.AppendFormat("&response_type={0}", "code");
            return builder.ToString();
            
        }

        public void RetrieveCode(CookieContainer container, string url)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = container, UseCookies = true }))
            {
                var response = client.GetAsync(url).Result;
                string code = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
                code = code.Split(';')[0].Split('=')[1];
                AuthenticateWithCode(code);
            }

        }

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
            _token = JsonConvert.DeserializeObject<AccessToken>(content);
            SaveToken(content);
            Connect();
        }

        public void Disconnect()
        {
            try
            {
                _file.DeleteAsync().Wait();
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
                var response = _client.GetAsync("https://www.google.com/accounts/OAuthLogin?source=wtalk&issueuberauth=1").Result;
                if (response.IsSuccessStatusCode)
                {
                    string uberAuth = response.Content.ReadAsStringAsync().Result;
                    response = _client.GetAsync(string.Format("https://accounts.google.com/MergeSession?service=mail&continue=http://www.google.com&uberauth={0}", uberAuth)).Result;
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
            _token = JsonConvert.DeserializeObject<AccessToken>(content);
            SaveToken(content);
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
