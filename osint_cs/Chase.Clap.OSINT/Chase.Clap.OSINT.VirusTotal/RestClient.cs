using System.Net.Http;

namespace Chase.Clap.OSINT.VirusTotal
{
    public sealed class RestClient
    {
        private const string apikey = "x-apikey";
        private const string v3BaseUrl = "https://www.virustotal.com/api/v3/";
        private readonly HttpClient client = new HttpClient();
        private readonly KeyRing keys;

        private const string urlCollection = "urls/";
        private const string ipCollection = "ips/";

        public RestClient(KeyRing keys)
        {
            this.keys = keys;
            client.DefaultRequestHeaders.Add(apikey, apikey);
        }

        public HttpResponseMessage MakeRequest(string url)
        {
            if (url != null)
            {
                url = Encode(url);
                if (!string.IsNullOrEmpty(url))
                {
                    client.DefaultRequestHeaders.Remove(apikey);
                    client.DefaultRequestHeaders.Add(apikey, keys.Next);
                    return client.GetAsync(v3BaseUrl + urlCollection + url).GetAwaiter().GetResult();
                }
            }
            return null;
        }

        internal string Encode(string url)
        {
            return Base64UrlEncoder.Encode(url).Replace("=", "");
        }
    }
}
