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
        public const string CLIENT_ID = "936475272427.apps.googleusercontent.com";
        public const string CLIENT_SECRET = "KWsJlkaMn1jGLxQpWxMnOox-";
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";


        //string[] scopes = new string[] { "https://www.googleapis.com/auth/chat", "https://www.googleapis.com/auth/client_channel", "https://www.googleapis.com/auth/googlevoice", "https://www.googleapis.com/auth/hangouts", "https://www.googleapis.com/auth/photos", "https://www.googleapis.com/auth/plus.circles.read", "https://www.googleapis.com/auth/plus.contactphotos", "https://www.googleapis.com/auth/plus.me", "https://www.googleapis.com/auth/plus.peopleapi.readwrite", "https://www.googleapis.com/auth/youtube.readonly" };
        string[] scopes = new string[] { "https://www.google.com/accounts/OAuthLogin" };


        HttpClient _client;

        // not a good idea ...
        
        AccessToken _token;

        public bool IsAuthenticated { get; private set; }

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

        public AuthenticationManager (string accessToken)
        {
            _token = new AccessToken() { access_token = accessToken };
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
            //builder.Append("https://accounts.google.com/o/oauth2/device/code?");
            builder.Append("https://accounts.google.com/o/oauth2/auth?");
            builder.AppendFormat("client_id={0}", CLIENT_ID);
            //builder.AppendFormat("&scope={0}", System.Uri.EscapeDataString("https://www.google.com/accounts/OAuthLogin"));//"https://www.googleapis.com/auth/googletalk"));
            builder.AppendFormat("&scope={0}", string.Join(" ", scopes));
            builder.AppendFormat("&redirect_uri={0}", REDIRECT_URI);
            builder.AppendFormat("&response_type={0}", "code");
            return builder.ToString();

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
            var response = _client.PostAsync("https://accounts.google.com/o/oauth2/token", c).Result;
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

        public void Connect()
        {
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

            var response = _client.PostAsync("https://accounts.google.com/o/oauth2/token", c).Result;
            string content = response.Content.ReadAsStringAsync().Result;
            _token = JsonConvert.DeserializeObject<AccessToken>(content);
            SaveToken(content);
        }


    }

    internal class AccessToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }

}
