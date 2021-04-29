using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MemberMailbox.Services
{
    public class ClientProxy
    {
        private readonly HttpClient http = new();

        private string RawCookie { get; init; }

        public ClientProxy(string rawCookie)
        {
            RawCookie = rawCookie;
        }

        private string GetRawCookie() => RawCookie;

        public Dictionary<string, string> GetCookie()
        {
            string rawcookie = GetRawCookie();
            Dictionary<string, string> result = new();

            rawcookie.Split(';').ToList()
                .ForEach(s =>
                {
                    var kp = s.Trim().Split('=');
                    result.Add(kp[0].Trim(), kp[1].Trim());
                });

            return result;
        }

        public async Task<HttpResponseMessage> PostAsync(Dictionary<string, string> para, string url)
        {
            var cookies = GetCookie();

            para.Add("csrf", cookies["bili_jct"]);
            para.Add("csrf_token", cookies["bili_jct"]);

            var content = new FormUrlEncodedContent(para);
            content.Headers.Add("cookie", GetRawCookie());

            return await http.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var content = new HttpRequestMessage(HttpMethod.Get, url);
            content.Headers.Add("cookie", GetRawCookie());

            return await http.SendAsync(content);
        }
    }
}
