using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Wallet {
    class HttpRequest {
        HttpClient httpClient = new HttpClient();
        public JObject Get(string _method, string _params, int _id = 1, string _node = "http://212.64.42.147:40332") {
            string url = _node + "/?jsonrpc=2.0&method=" + _method + "&params=[\"" + _params + "\"]&id=" + 1;
            string json = httpClient.GetStringAsync(url).Result;
            return JObject.Parse(json);
        }
        public JObject Post(string _method, string _params, int _id = 1, string _node = "http://212.64.42.147:40332") {
            string body = "{\"jsonrpc\": \"2.0\",\"method\": \"" + _method + "\",\"params\": [\"" + _params + "\"],\"id\": " + _id + "}";
            HttpResponseMessage httpResponseMessage = httpClient.PostAsync(_node, new StringContent(body)).Result;
            string json = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return JObject.Parse(json);
        }
        public JObject GetBlock(long blockNum, string node = "http://212.64.42.147:40332") {
            string url = node + "/?jsonrpc=2.0&method=getblock&params=[" + blockNum + ",1]&id=" + 1;
            string json = httpClient.GetStringAsync(url).Result;
            return JObject.Parse(json);
        }
    }
}