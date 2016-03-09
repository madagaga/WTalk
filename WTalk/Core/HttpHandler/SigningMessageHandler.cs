using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WTalk.Core.Utils;

using System.Reflection;
using System.Reflection.Emit;

namespace WTalk.Core.HttpHandler
{
    public class SigningMessageHandler : DelegatingHandler
    {
        static Uri _cookieUri = new Uri(HangoutUri.COOKIE_URI);

        public SigningMessageHandler()
            : base(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = Client.CookieContainer, UseCookies = true })
        {

        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("UserAgent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.132 Safari/537.36");
            request.Headers.Add("X-Origin", HangoutUri.ORIGIN_URL);
            request.Headers.Add("X-Goog-Authuser", "0");
            request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("utf-8"));
            Cookie sapsid = Client.CookieContainer.GetCookies(request.RequestUri).Cast<Cookie>().FirstOrDefault(c => c.Name == "SAPISID");

            if (sapsid != null)
            {
                TimeSpan t = DateTime.UtcNow.TimeIntervalSince1970();
                string secondsSinceEpoch = t.TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

                string sapisidHash = string.Format("{0} {1} {2}", secondsSinceEpoch, sapsid.Value, HangoutUri.ORIGIN_URL);
                sapisidHash = sapisidHash.ComputeSHA1Hash();
                request.Headers.Add("Authorization", string.Format("SAPISIDHASH {0}_{1}", secondsSinceEpoch, sapisidHash));

            }
            return base.SendAsync(request, cancellationToken);
        }


    }
}
