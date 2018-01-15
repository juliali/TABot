using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WechatCommon.Data;
using WechatCommon.Exceptions;

namespace WechatCommon.Tencent
{
    public class IOParser
    {
        public WechatRequest ParseRequest(HttpRequestMessage request)
        {
            string requestXml = request.Content.ReadAsStringAsync().Result;
            //LogUtils.Log("[POST input]:\r\n" + requestXml);
            WechatRequest req = new WechatRequest();

            XmlDocument doc = new XmlDocument();
            XmlNode root;

            try
            {
                doc.LoadXml(requestXml);
                root = doc.FirstChild;
                req.ToUserName = root["ToUserName"].InnerText;
                req.FromUserName = root["FromUserName"].InnerText;
                req.CreateTime = root["CreateTime"].InnerText;
                req.MsgType = root["MsgType"].InnerText;
                req.Content = root["Content"].InnerText;
                req.MsgId = root["MsgId"].InnerText;
         
                if (req.ToUserName == null || req.MsgId == null || req.Content == null)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw new WebResponseException(HttpStatusCode.BadRequest, $"Missing field: ToUserName - {req.ToUserName}, MsgId - {req.MsgId}, Content - {req.Content}");
            }

            return req;
        }

        public string GenerateEncryptResponse(WechatRequest req, string replyMsg)
        {            
            WechatResponse resp = new WechatResponse();
            resp.FromUserName = req.ToUserName;
            resp.ToUserName = req.FromUserName;
            resp.CreateTime = DateTime.Now.Ticks.ToString();
            resp.MsgType = req.MsgType;
            resp.Content = req.MsgType == "text" ? replyMsg : "";

            string encryptResp = this.EncryptXML(resp);
            return encryptResp;
        }

        private string EncryptXML(WechatResponse resp)
        {
            string sEncryptMsg = "";

            string ToUserNameLabelHead = "<ToUserName><![CDATA[";
            string ToUserNameLabelTail = "]]></ToUserName>";

            string FromUserNameLabelHead = "<FromUserName><![CDATA[";
            string FromUserNameLabelTail = "]]></FromUserName>";
            string CreateTimeLabelHead = "<CreateTime><![CDATA[";
            string CreateTimeLabelTail = "]]></CreateTime>";
            string MsgTypeLabelHead = "<MsgType><![CDATA[";
            string MsgTypeLabelTail = "]]></MsgType>";
            string ContentLabelHead = "<Content><![CDATA[";
            string ContentLabelTail = "]]></Content>";

            sEncryptMsg += "<xml>" + ToUserNameLabelHead + resp.ToUserName + ToUserNameLabelTail;
            sEncryptMsg += FromUserNameLabelHead + resp.FromUserName + FromUserNameLabelTail;
            sEncryptMsg += CreateTimeLabelHead + resp.CreateTime + CreateTimeLabelTail;
            sEncryptMsg += MsgTypeLabelHead + resp.MsgType + MsgTypeLabelTail;
            sEncryptMsg += ContentLabelHead + resp.Content + ContentLabelTail;
            sEncryptMsg += "</xml>";
            
            return sEncryptMsg;
        }

        

    }
}
