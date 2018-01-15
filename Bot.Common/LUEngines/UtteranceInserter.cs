using Bot.Common.Data;
using Bot.Common.LUEngine.Luis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bot.Common.LUEngines
{
    public class UtteranceInserter
    {
        private readonly string DefaultAppId = ConfigurationManager.AppSettings["LuisAppId"];
        private readonly string DefaultSubscriptionKey = ConfigurationManager.AppSettings["LuisSubscription"];

        public HttpResponseMessage MakeRequest(IntentPair[] body)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", DefaultSubscriptionKey);

            var uri = //"https://westus.api.cognitive.microsoft.com/luis/v1.0/prog/apps/" + DefaultAppId + "/examples?" + queryString;
                "https://westus.api.cognitive.microsoft.com/luis/api/v2.0//apps/" + DefaultAppId + "/versions/0.1/examples";

            HttpResponseMessage response;

            string bodyStr = JsonConvert.SerializeObject(body);
            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(bodyStr);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                Task<HttpResponseMessage> resp = client.PostAsync(uri, content);

                response = resp.Result;
                return response;
            }            

        }

        public List<IntentPair> ReadIntentTrainingFile(string filePath)
        {
            List<IntentPair> results = new List<IntentPair>();

            string[] lines = System.IO.File.ReadAllLines(filePath);

            foreach(string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] tmps = line.Split('\t');

                if (tmps.Length < 3)
                {
                    continue;
                }

                IntentPair pair = new IntentPair();

                pair.text = tmps[1];
                pair.intentName = tmps[2];

                results.Add(pair);
            }

            return results;
        }
    }
}
