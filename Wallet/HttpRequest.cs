using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace Wallet {
    class HttpRequest {
        HttpClient httpClient = new HttpClient();
        public JObject Get(string url) {
            string json = httpClient.GetStringAsync(url).Result;
            return JObject.Parse(json);
        }
        public JObject Post(string boby, string url = "http://127.0.0.1:20337") {
            HttpResponseMessage httpResponseMessage = httpClient.PostAsync(url, new StringContent(boby)).Result;
            string json = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return JObject.Parse(json);
        }
    }
}