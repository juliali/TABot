using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WechatBotFramework.Component
{
    public class WechatHttpClient
    {
        private static ILog log = LogManager.GetLogger("WechatHttpClient");
        private CookieContainer cookieContainer = new CookieContainer();

        public string GetString(string url)
        {
            return GetString(url, null);
        }

        public string GetString(string url, Dictionary<string, string> queryParams)
        {
            DateTime startTime = DateTime.Now;
            string queryStr = "";
            if (queryParams != null && queryParams.Count > 0)
            {
                foreach (string key in queryParams.Keys)
                {
                    queryStr += key + "=" + queryParams[key] + "&";
                }

                queryStr = queryStr.Substring(0, queryStr.Length - 1);
            }

            url += queryStr;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.CookieContainer = cookieContainer; // <= HERE
                req.Method = "GET";
                req.KeepAlive = false;

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                string result = null;
                if (resp.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = resp.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        result = reader.ReadToEnd();
                    }
                }
                resp.Close();

                DateTime endTime = DateTime.Now;

                string latencyStr = endTime.Subtract(startTime).TotalSeconds.ToString();

                log.Debug("It took " + latencyStr + " seconds to send <GET> to wechat server.");

                return result;

            }
            catch (WebException e)
            {
                log.Error(e.Message);
                return null;
            }

        }

        public string Post(string url, string paramJsonStr, int timeoutSeconds)
        {
            DateTime startTime = DateTime.Now;
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.CookieContainer = cookieContainer; // <= HERE
                req.Method = "POST";
                req.KeepAlive = false;

                req.ContentType = "application/json";

                if (timeoutSeconds > 0)
                {
                    req.Timeout = timeoutSeconds * 1000;
                }


                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    streamWriter.Write(paramJsonStr);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)req.GetResponse();
                string result = null;
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                httpResponse.Close();

                DateTime endTime = DateTime.Now;

                string latencyStr = endTime.Subtract(startTime).TotalSeconds.ToString();

                log.Debug("It took " + latencyStr + " seconds to send <POST> to wechat server.");
                return result;

            }
            catch (WebException e)
            {
                log.Error(e.Message);
                return null;
            }

        }

        public string Post(string url, string paramJsonStr)
        {
            return Post(url, paramJsonStr, -1);
        }
    }
}
