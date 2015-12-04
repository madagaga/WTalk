using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WTalk.Utils;
namespace WTalk.HttpHandler
{
    public class SigningMessageHandler : DelegatingHandler
    {        
        public SigningMessageHandler()
            : base(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = Client.CookieContainer, UseCookies = true })
        {
            
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("UserAgent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.132 Safari/537.36");
            request.Headers.Add("X-Origin", HangoutUri.ORIGIN_URL);
            request.Headers.Add("X-Goog-Authuser", "0");

            Cookie sapsid = Client.CookieContainer.GetCookie(".google.com", "SAPISID");
            if (sapsid != null)
            {
                TimeSpan t = DateTime.UtcNow.TimeIntervalSince1970();
                string secondsSinceEpoch = t.TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

                string sapisidHash = string.Format("{0} {1} {2}", secondsSinceEpoch, sapsid.Value, HangoutUri.ORIGIN_URL);
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] buffer = System.Text.Encoding.Default.GetBytes(sapisidHash);
                    sapisidHash = sha1.ComputeHash(buffer).ToHex();
                    request.Headers.Add("Authorization", string.Format("SAPISIDHASH {0}_{1}", secondsSinceEpoch, sapisidHash));
                }
            }
            return base.SendAsync(request, cancellationToken);
        }

    }
}
