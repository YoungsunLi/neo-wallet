using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Wallet {
    class HttpRequest {
        HttpClient httpClient = new HttpClient();
        public JObject Get(string url) {
            string json = httpClient.GetStringAsync(url).Result;
            return JObject.Parse(json);
        }
    }
}
