using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OutlookAddInThreadsAndQueue.CheckUrlSecurity.server
{
   public  class CheckPointSecureUrlClient
    {
        private string token { get; set; }
        public CheckPointSecureUrlClient()
        {
            this.token = getToken();
        }
        public string getToken()
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpRequestMessage request = new HttpRequestMessage();
                HttpResponseMessage response = new HttpResponseMessage();
                request.RequestUri = new Uri("https://rep.checkpoint.com/rep-auth/service/v1.0/request");
                request.Method = HttpMethod.Get;
                request.Headers.Add("Client-Key", "2fddabf7-ac89-4ba5-9c1d-69af31f86408");
                response = client.SendAsync(request).Result;
                string token = response.Content.ReadAsStringAsync().Result;
                return token;

            }
            catch (HttpRequestException e) { }
            return "server error";
        }

        public string PostAsyncfunc(string url)
        {
            string content = "{\"request\":[{ \"resource\": \"" + url + "\"}]}";
            string urlToCheck = @"https://rep.checkpoint.com/url-rep/service/v2.0/query?resource={" + url + "}";
            byte[] data = Encoding.UTF8.GetBytes(content);
            WebRequest request = WebRequest.Create(urlToCheck);
            request.Method = HttpMethod.Post.ToString();
            request.Headers.Add("Client-Key", "2fddabf7-ac89-4ba5-9c1d-69af31f86408");
            request.Headers.Add("token", token);
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                WebResponse response = request.GetResponse();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();
                    JObject adResponse =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseContent);
                    string status = (string)adResponse["response"][0]["status"]["label"];
                    string risk = (string)adResponse["response"][0]["risk"];
                    if (status == "SUCCESS" && Convert.ToInt32(risk) < 20) { return "url is not malicious"; }
                    return "url is malicious";

                }
            }
            catch (WebException webException)
            {
                if (webException.Response != null)
                {
                    using (StreamReader reader = new StreamReader(webException.Response.GetResponseStream()))
                    {
                        string responseContent = reader.ReadToEnd();
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseContent).ToString(); ;
                    }
                }
            }

            return null;
        }
    }
}
