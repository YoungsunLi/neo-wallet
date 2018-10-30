using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Wallet {
    class HttpRequest {
        public JObject Get(string _method, string _params, int _id = 1, string _node = "http://212.64.42.147:40332") {
            using(HttpClient httpClient = new HttpClient()) {
                string url = _node + "/?jsonrpc=2.0&method=" + _method + "&params=[\"" + _params + "\"]&id=" + 1;
                string result = httpClient.GetStringAsync(url).Result;
                return JObject.Parse(result);
            }
        }
        public JObject Post(string _method, string _params, int _id = 1, string _node = "http://212.64.42.147:40332") {
            using(HttpClient httpClient = new HttpClient()) {
                string body = "{\"jsonrpc\": \"2.0\",\"method\": \"" + _method + "\",\"params\": [\"" + _params + "\"],\"id\": " + _id + "}";
                HttpResponseMessage httpResponseMessage = httpClient.PostAsync(_node, new StringContent(body)).Result;
                string result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                return JObject.Parse(result);
            }
        }
        public JObject GetBlock(long _blockNum, string _node = "http://212.64.42.147:40332") {
            using(HttpClient httpClient = new HttpClient()) {
                string url = _node + "/?jsonrpc=2.0&method=getblock&params=[" + _blockNum + ",1]&id=" + 1;
                string result = httpClient.GetStringAsync(url).Result;
                return JObject.Parse(result);
            }
        }
    }
}