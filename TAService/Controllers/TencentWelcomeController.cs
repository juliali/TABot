using TA.Engine.Controllers;
using TA.Engine.Services;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WechatCommon.Data;
using WechatCommon.Exceptions;
using WechatCommon.Tencent;

namespace TAService.Controllers
{
    public class TencentWelcomeController : ApiController
    {
        private readonly WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(Constants.Token, Constants.EncodingAESKey, ConfigurationManager.AppSettings["WechatAppId"]);
        private IntentController bot = new IntentController();
        private IOParser parser = new IOParser();

        public HttpResponseMessage Get(string signature = "",  string timestamp="", string nonce="", string echostr = "")
        {            
            string querystr = string.Join("&",
                HttpContext.Current.Request.QueryString
               .AllKeys
               .Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]).ToArray());
           
            int ret = wxcpt.VerifyURL(signature, timestamp, nonce);           

            if (ret != 0)
            {
                throw new WebResponseException(HttpStatusCode.InternalServerError, $"VerifyURL failed: {ret}");                
            }
            
            string resp_echostr = echostr;

            HttpResponseMessage resp = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(resp_echostr, System.Text.Encoding.UTF8, "text/plain") };                      
            
            return resp;
        }

        public HttpResponseMessage Post(HttpRequestMessage request)
        {           
            try
            {
                WechatRequest req = parser.ParseRequest(request);

                string answer = this.Answer(req);

                string str_encrypt = parser.GenerateEncryptResponse(req, answer);
                
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(str_encrypt, System.Text.Encoding.UTF8, "text/xml") };

            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }

        private string Answer(WechatRequest req)
        {
            string answer = string.Empty;

            if (req.MsgType == "text")
            {
                answer = this.bot.Answer(req.FromUserName, req.Content);
            }

            return answer;
        }

    }

    public class Constants
    {
        public static string Token = "bjxx20170510abc"; 
        public static string EncodingAESKey = "2oBfG2bvFbRH3m4hOqgPnXvWqBI81OEWmiiuBJoQ6YS";        
    }    
}