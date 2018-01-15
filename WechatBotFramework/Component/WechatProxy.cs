using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Gma.QrCodeNet.Encoding;
using System.Drawing;
using System.Drawing.Imaging;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WechatBotFramework.Data;
using log4net;

namespace WechatBotFramework.Component
{
    class BaseRequest
    {
        public int Uin { get; set; }
        public string Sid { get; set; }
        public string Skey { get; set; }
        public string DeviceID { get; set; }
    }

    public class WechatProxy
    {
        private static ILog log = LogManager.GetLogger("WechatProxy");

        private string wxqrFilePath = "D:\\data\\psa\\test\\wxqr.png";
        private string uuid = null;

        private string deviceId = "e642833123359051"; 
        private string redirectURI = null;
        private string baseURI = null;
        private string baseHost = null;

        private string skey = null;
        private string sid = null;
        private string uin = null;
        private string pass_ticket = null;

        private BaseRequest baseRequest = null;

        private SyncKeyInfo sync_key = null;
        private ContactInfo myAccount = null;
        private string sync_key_str = null;

        private Dictionary<string, string> encryChatRoomIdList = new Dictionary<string, string>();

        // private List<ContactInfo> groupList = new List<ContactInfo>();
        // private Dictionary<string, Dictionary<string, ContactInfo>> groupMembers = new Dictionary<string, Dictionary<string, ContactInfo>>();        
        // private Dictionary<string, ContactInfoWithType> normalAccountInfo = new Dictionary<string, ContactInfoWithType>();                

        private static UserMgmtStore userMgmtStore = UserMgmtStore.Instance;

        private string sync_host;

        private string status;

        private const string UNKONWN = "unkonwn";
        private const string SUCCESS = "200";
        private const string SCANED = "201";
        private const string TIMEOUT = "408";

        private const string NORMALMEMBER = "NormalMember";
        private const string GROUPMEMBER = "GroupMember";

        WechatHttpClient wechatClient = new WechatHttpClient();

        public WechatProxy()
        { }

        public WechatProxy(string wxqrFilePath)
        {
            this.wxqrFilePath = wxqrFilePath;
        }

        private string GetTimeStamp()
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            int timestamp = (int)t.TotalSeconds;
            return timestamp.ToString();
            //DateTime.Now.Millisecond.ToString();//(DateTime.Now.ToFileTime() * 1000 + (new Random()).Next(999)).ToString();
        }

        public void GetUUID()
        {
            //string uuid = null;

            string url = "https://login.weixin.qq.com/jslogin";
            Dictionary<string, string> Params = new Dictionary<string, string>(){
            {"appid", "wx782c26e4c19acffb" },
            {"fun", "new"},
            { "lang", "zh_CN" },
            { "_", GetTimeStamp()}
            };

            WebClient webClient = new WebClient();
            foreach(string key in Params.Keys)
            {
                webClient.QueryString.Add(key, Params[key]);
            }
                        
            string result = webClient.DownloadString(url);                        

            string rule = "window.QRLogin.code = (\\d+); window.QRLogin.uuid = \"(\\S+?)\"";
            Regex regex = new Regex(rule);

            Match match = regex.Match(result);
            string code = null;
            while (match.Success)
            {
                code = match.Groups[1].ToString();
                if (code == "200")
                {                
                    this.uuid = match.Groups[2].ToString();
                    return;
                }

                match = match.NextMatch();
            }
            
        }

        public void GenerateQRCode()
        {
            string str = "https://login.weixin.qq.com/l/" + this.uuid;           

            QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode qrCode = new QrCode();
            qrEncoder.TryEncode(str, out qrCode);

            GraphicsRenderer renderer = new GraphicsRenderer(new FixedCodeSize(400, QuietZoneModules.Zero), Brushes.Black, Brushes.White);

            MemoryStream ms = new MemoryStream();

            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, ms);

            var imageTemp = new Bitmap(ms);
            int image_size = 200;

            var image = new Bitmap(imageTemp, new Size(new Point(image_size, image_size)));

            image.Save(this.wxqrFilePath, ImageFormat.Png);
        }

        public void ShowWechatLoginQR()
        {
            Process.Start(this.wxqrFilePath);
        }

        private string GetResponseCode(string respStr)
        {            
            string result = ParsePatternFromText("window.code=(\\d+);", respStr, 1);
            if (result == null)
            {
                return UNKONWN;
            }
            else
            {
                return result;
            }
        }

        private string ParsePatternFromText(string rule, string text, int groupIndex)
        {
            Regex regex = new Regex(rule);

            Match match = regex.Match(text);
            string result = null;
            while (match.Success)
            {
                result = match.Groups[groupIndex].ToString();
                return result;

                match = match.NextMatch();
            }
            return null;
        }

        public string WaitForLogin()
        {
            int tip = 1;
            int tryLaterSecs = 1;
            int MAXRETRYTIMES = 10;

            string code = UNKONWN;

            int retryTime = MAXRETRYTIMES;

            while (retryTime > 0)
            {
                string url = "https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?tip=" + tip.ToString() + "&uuid=" + this.uuid + "&_=" + GetTimeStamp();

                //WebClient webClient = new WebClient();

                string result = wechatClient.GetString(url);//webClient.DownloadString(url);                

                code = GetResponseCode(result);

                if (code == SCANED)
                {
                    log.Info("[INFO] Please confirm to login .");
                    tip = 0;
                }
                else if (code == SUCCESS)
                {
                    this.redirectURI = ParsePatternFromText("window.redirect_uri=\"(\\S+?)\";", result, 1) + "&fun=new";
                    this.baseURI = this.redirectURI.Split('?')[0];

                    string[] tmps = this.baseURI.Split('/');
                    this.baseURI = this.baseURI.Replace("/" + tmps[tmps.Length - 1], "");

                    this.baseHost = this.baseURI.Replace("https://", "").Split('/')[0];
                    return code;                    
                }
                else if (code == TIMEOUT)
                {
                    log.Error(" [ERROR] WeChat login timeout. retry in " + tryLaterSecs.ToString() + " secs later...");
                    tip = 1;
                    retryTime -= 1;
                    try
                    {
                        Thread.Sleep(tryLaterSecs * 1000);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.Message);
                    }
                }
                else
                {
                    log.Error("[ERROR] WeChat login exception return_code=" + code + ". retry in " + tryLaterSecs.ToString() + " secs later...");
                    tip = 1;
                    retryTime -= 1;
                    try
                    {
                        Thread.Sleep(tryLaterSecs * 1000);
                    }
                    catch (Exception e)
                    {
                        log.Error(e.Message);
                    }
                }

            }
            return code;
        }

        public bool Login()
        {
            //WebClient webClient = new WebClient();
            string result = wechatClient.GetString(this.redirectURI); // webClient.DownloadString(this.redirectURI);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                switch(node.Name)
                {
                    case "skey":
                        this.skey = node.ChildNodes[0].Value;
                        break;
                    case "wxsid":
                        this.sid = node.ChildNodes[0].Value;
                        break;
                    case "wxuin":
                        this.uin = node.ChildNodes[0].Value;
                        break;
                    case "pass_ticket":
                        this.pass_ticket = node.ChildNodes[0].Value;
                        break;
                }
            }

            if (this.skey == null || this.sid == null || this.uin == null || this.pass_ticket == null)
            {
                return false;
            }

            this.baseRequest = new BaseRequest();
            this.baseRequest.Uin = int.Parse(this.uin);
            this.baseRequest.Sid = this.sid;
            this.baseRequest.Skey = this.skey;
            this.baseRequest.DeviceID = this.deviceId;            

            return true;
        }

        class InitRequestParam
        {
            public BaseRequest BaseRequest;
        }

        public bool Init()
        {
        
            string url = this.baseURI + "/webwxinit?r=" + this.GetTimeStamp() + "&lang=en_US&pass_ticket=" + this.pass_ticket;
            //WebClient webClient = new WebClient();

            //webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

            InitRequestParam reqparm = new InitRequestParam();
            reqparm.BaseRequest = this.baseRequest;

            string baseRequestStr = JsonConvert.SerializeObject(reqparm);

            string result = wechatClient.Post(url, baseRequestStr);//webClient.UploadString(url, baseRequestStr);
                                  
            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);

            this.sync_key = JsonConvert.DeserializeObject<SyncKeyInfo>(json.SyncKey.ToString());//JsonConvert.SerializeObject(json.SyncKey); 
            this.myAccount = JsonConvert.DeserializeObject<ContactInfo>(json.User.ToString()); 

            List<string> strList = new List<string>();
            
            foreach(KeyVal element in this.sync_key.List)
            {
                string str = element.Key + "_" + element.Val; 
                strList.Add(str);
            }

            this.sync_key_str = string.Join("|", strList);

            int ret = json.BaseResponse.Ret; 
            if (ret == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        class NotifyStatusRequestParam
        {
            public BaseRequest BaseRequest;
            public int Code = 3;
            public string FromUserName;
            public string ToUserName;
            public long ClientMsgId;
        }

        public bool NotifyStatus()
        {
            string url = this.baseURI + "/webwxstatusnotify?lang=zh_CN&pass_ticket=" + this.pass_ticket;
            NotifyStatusRequestParam reqParam = new NotifyStatusRequestParam();
            reqParam.BaseRequest = this.baseRequest;

            //dynamic myaccountJson = JsonConvert.DeserializeObject<dynamic>(this.myAccount);

            reqParam.FromUserName = this.myAccount.UserName;
            reqParam.ToUserName = this.myAccount.UserName;
            reqParam.ClientMsgId = long.Parse(this.GetTimeStamp());


           // WebClient webClient = new WebClient();
           // webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            
            string paramStr = JsonConvert.SerializeObject(reqParam);

            string result = wechatClient.Post(url, paramStr);//webClient.UploadString(url, paramStr);

            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);

            int ret = json.BaseResponse.Ret;
            if (ret == 0)
            {
                return true;
            }
            else
            {
                return false;
            }           
        }        


        private int ReadMembers(ref List<ContactInfo> memberList, string result)
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
            int ret = json.BaseResponse.Ret;
            if (ret == 0)
            {
                foreach (var member in json.MemberList)
                {
                    string jsonStr = member.ToString();
                    ContactInfo dictMemeber = JsonConvert.DeserializeObject<ContactInfo>(jsonStr);
                    memberList.Add(dictMemeber);
                }

                string seqStr = json.Seq.ToString();
                int seq = int.Parse(seqStr);

                return seq;
            }
            else
            {
                return -1;
            }
            
        }
                
        public bool GetContacts()
        {
            string url = this.baseURI + "/webwxgetcontact?seq=0&pass_ticket=" + this.pass_ticket + "&skey=" + this.skey + "&r=" + this.GetTimeStamp();            

            string paramStr = "{}";

            List<ContactInfo> memberList = new List<ContactInfo>();
            try
            {
                string result = wechatClient.Post(url, paramStr, 180);
                                              
                int seq = this.ReadMembers(ref memberList, result);
                
                while (seq != 0)
                {                    
                    url = this.baseURI + "/webwxgetcontact?seq=" + seq.ToString() + "&pass_ticket=" + this.pass_ticket + "&skey=" + this.skey + "&r=" + this.GetTimeStamp();
                    result = wechatClient.Post(url, paramStr, 180);

                    seq = this.ReadMembers(ref memberList, result);
                }

               // this.memberList.AddRange(memberList);

                string[] specialUsers = {"newsapp", "fmessage", "filehelper", "weibo", "qqmail",
                         "fmessage", "tmessage", "qmessage", "qqsync", "floatbottle",
                         "lbsapp", "shakeapp", "medianote", "qqfriend", "readerapp",
                         "blogapp", "facebookapp", "masssendapp", "meishiapp",
                         "feedsapp", "voip", "blogappweixin", "weixin", "brandsessionholder",
                         "weixinreminder", "wxid_novlwrv3lqwv11", "gh_22b87fa7cb3c",
                         "officialaccounts", "notification_messages", "wxid_novlwrv3lqwv11",
                         "gh_22b87fa7cb3c", "wxitil", "userexperience_alarm", "notification_messages"};
                
               
                foreach(ContactInfo contact in memberList)
                {
                    if ((contact.VerifyFlag & 8 ) != 0)
                    {
                        
                        ContactInfoWithType it = new ContactInfoWithType();
                        it.type = MessageType.Public;//"public";
                        it.info = contact;

                        //this.normalAccountInfo.Add(contact.UserName, it);
                        userMgmtStore.SetNoramlAccountInfo(contact.UserName, it);
                    }
                    else if (specialUsers.Contains(contact.UserName))
                    {               
                        
                        ContactInfoWithType it = new ContactInfoWithType();
                        it.type = MessageType.Special;// "special";
                        it.info = contact;

                        //this.normalAccountInfo.Add(contact.UserName, it);
                        userMgmtStore.SetNoramlAccountInfo(contact.UserName, it);
                    }
                    else if (this.myAccount.UserName == contact.UserName)
                    {
                        ContactInfoWithType it = new ContactInfoWithType();
                        it.type = MessageType.Self;//"self";
                        it.info = contact;

                        //this.normalAccountInfo.Add(contact.UserName, it); 
                        userMgmtStore.SetNoramlAccountInfo(contact.UserName, it);
                    }
                    else if (contact.UserName.Contains("@@"))
                    {
                        //this.groupList.Add(contact);
                        userMgmtStore.AddGroup(contact);

                        ContactInfoWithType it = new ContactInfoWithType();
                        it.type = MessageType.Group;//"group";
                        it.info = contact;

                        //this.normalAccountInfo.Add(contact.UserName, it);
                        userMgmtStore.SetNoramlAccountInfo(contact.UserName, it);
                    }
                    else
                    {                     
                        ContactInfoWithType it = new ContactInfoWithType();
                        it.type = MessageType.Contact;//"contact";
                        it.info = contact;

                        //this.normalAccountInfo.Add(contact.UserName, it);
                        userMgmtStore.SetNoramlAccountInfo(contact.UserName, it);
                    }
                }
                
                return true;
                
            } catch(Exception e)
            {
                log.Error(e.Message);
                return false;
            }
        }

        class GroupRequestParam
        {
            public BaseRequest BaseRequest;
            public int Count;
            public GroupData[] List;
        }

        class GroupData
        {
            public string UserName;
            public string EncryChatRoomId = "";
        }

        private int ReadGroupMembers(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return 1;
            }

            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
            int ret = json.BaseResponse.Ret;
            if (ret == 0)
            {

                foreach (var group in json.ContactList)
                {
                    string gid = group.UserName;
                    Dictionary<string, ContactInfo> memberList = new Dictionary<string, ContactInfo>();

                    foreach (var member in group.MemberList)
                    {
                        string jsonStr = member.ToString();
                        ContactInfo dictMemeber = JsonConvert.DeserializeObject<ContactInfo>(jsonStr);
                        memberList.Add(dictMemeber.UserName, dictMemeber);

                        string memberUserName = member.UserName;                        
                    }

                    string encryChatRoomId = group.EncryChatRoomId;

                    //this.groupMembers.Add(gid,memberList);
                    userMgmtStore.SetGroupMembers(gid, memberList);

                    this.encryChatRoomIdList[gid] = encryChatRoomId;
                   
                }

                return 0;
            }
            else
            {
                return -1;
            }

        }

        public bool BatchGetGroupMembers()
        {
            string url = this.baseURI + "/webwxbatchgetcontact?type=ex&r=" + this.GetTimeStamp() + "&pass_ticket=" + this.pass_ticket; // % (int(time.time()), self.pass_ticket);

            GroupRequestParam param = new GroupRequestParam();
            param.BaseRequest = this.baseRequest;
            param.Count = userMgmtStore.GetGroupNumber();//this.groupList.Count;
            param.List = new GroupData[param.Count];
            for(int i = 0; i < param.Count; i ++)
            {
                GroupData info = new GroupData();
                ContactInfo group = userMgmtStore.GetGroup(i);
                info.UserName = group.UserName;//this.groupList[i].UserName;
                info.EncryChatRoomId = string.IsNullOrEmpty(group.EncryChatRoomId)?"": group.EncryChatRoomId;
                param.List[i] = info;
            }

            string paramStr = JsonConvert.SerializeObject(param);
            string result = wechatClient.Post(url, paramStr);

            int ret = 1;

            try
            { 
                ret = this.ReadGroupMembers(result);
            }
            catch(Exception e)
            {
                log.Error(e.Message);
            }

            if (ret == 0)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        private bool TestSyncCheck()
        {
            string[] hosts = {"webpush." , "webpush2." };

            string retcode = null;

            foreach (string host in hosts)
            {
                this.sync_host = host + this.baseHost;
                SyncInfo info = new SyncInfo();
                try
                {
                    info = SyncCheck();
                    retcode = info.retcode;    
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                    retcode = "-1";
                }

                if (retcode == "0")
                {
                    return true;
                }
            }

            return false;
        }

        class SyncInfo
        {
            public string retcode = "";
            public string selector = "";
        }

        private SyncInfo SyncCheck()
        {
            SyncInfo info = new SyncInfo();

            string url = "https://" + this.sync_host + "/cgi-bin/mmwebwx-bin/synccheck?r=" + this.GetTimeStamp() +
               "&sid=" + this.sid + "&uin=" + this.uin + "&skey=" + this.skey + "&deviceid=" + this.deviceId + "&synckey=" + this.sync_key_str + "&_=" + this.GetTimeStamp();

            string result = wechatClient.GetString(url);

            string rule = "window.synccheck={retcode:\"(\\d+)\",selector:\"(\\d+)\"}";
            Regex regex = new Regex(rule);

            Match match = regex.Match(result);
            
            if (match.Success)
            {
                info.retcode = match.Groups[1].ToString();
                info.selector = match.Groups[2].ToString();
            }

            if (info.retcode != "0")
            {
                log.Warn(result);                
            }

            //log.Info("retcode: " + info.retcode + "; selector: " + info.selector);
            return info;
        }

        private void Schedule()
        {
            /// TODO
        }

        class SyncData
        {
            public BaseRequest BaseRequest;
            public SyncKeyInfo SyncKey;
            public int rr;
        }
        private string Sync()
        {
            string url = this.baseURI + "/webwxsync?sid=" + this.sid + "&skey=" + this.skey + "&lang=en_US&pass_ticket=" + this.pass_ticket;

            SyncData Params = new SyncData();
            Params.BaseRequest = this.baseRequest;
            Params.SyncKey = this.sync_key;
            Params.rr = int.Parse(this.GetTimeStamp());

            string paramStr = JsonConvert.SerializeObject(Params);

            try
            { 
                string result = wechatClient.Post(url, paramStr, 60);

                dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
                int ret = json.BaseResponse.Ret;
                if (ret == 0)
                {                    
                    this.sync_key = JsonConvert.DeserializeObject<SyncKeyInfo>(json.SyncKey.ToString());
                    
                    List<string> strList = new List<string>();

                    foreach (KeyVal element in this.sync_key.List)
                    {
                        string str = element.Key + "_" + element.Val;
                        strList.Add(str);
                    }

                    this.sync_key_str = string.Join("|", strList);

                    return result;
                }
            } catch (Exception e)
            {
               log.Error(e.Message);
            }
            return null;

        }

        private string SearchContent(string key, string content, string format)
        {
            string rule = "";
            if (string.IsNullOrWhiteSpace(format) || format == "attr")
            {
                rule = key + "\\s?=\\s?\"([^\"<]+)\"";
            }
            else
            {
                rule = "<" + key + ">([^<]+)</" + key + ">";
            }

            Regex regex = new Regex(rule);
            Match match = regex.Match(content);

            if (match.Success)
            {
                string result = match.Groups[1].ToString();
                return result;
            }

            return "unknown";
        }

        class TypeValueData
        {
            public string type;
            public string value;
        }

        private string[] ProcAtInfo(ref MessageInfo msgInfo, string msg)
        {
            if(string.IsNullOrWhiteSpace(msg))
            {
                return null;
            }

            string[] segs = msg.Split("\u2005".ToCharArray());

            string str_msg_all = "";
            string str_msg = "";
            List<TypeValueData> infos = new List<TypeValueData>();

            if (segs.Length > 1)
            {
                for (int i = 0; i < segs.Length; i ++ )
                {
                    string seg = segs[i];

                    if (i < segs.Length -1)
                    { 
                        seg += "\u2005";
                    }

                    string rule = "@.*\u2005";

                    Regex regex = new Regex(rule);
                    Match match = regex.Match(seg);
                    if (match.Success)
                    {
                        string pm = match.Groups[0].ToString();
                        string name = pm.Substring(1, pm.Length - 1);
                        string str = seg.Replace(pm, "");
                        str_msg_all += str + "@" + name + " ";
                        str_msg += str;

                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            TypeValueData tv = new TypeValueData();
                            tv.type = "str";
                            tv.value = str;

                            infos.Add(tv);
                        }
                        
                        TypeValueData tv2 = new TypeValueData();
                        tv2.type = "at";
                        tv2.value = name;

                        infos.Add(tv2);
                    }
                    else
                    {
                        str_msg_all += seg;
                        str_msg += seg;
                                               
                        TypeValueData tv3 = new TypeValueData();
                        tv3.type = "str";
                        tv3.value = seg;

                        infos.Add(tv3);
                    }
                }

                str_msg_all += segs[segs.Length - 1];
                str_msg += segs[segs.Length - 1];                

                TypeValueData tv4 = new TypeValueData();
                tv4.type = "str";
                tv4.value = segs[segs.Length - 1];

                infos.Add(tv4);

            }
            else
            {               
                TypeValueData tv5 = new TypeValueData();
                tv5.type = "str";
                tv5.value = segs[segs.Length - 1];

                infos.Add(tv5);

                str_msg_all = msg;
                str_msg = msg;
            }

            if (!string.IsNullOrWhiteSpace(msgInfo.user.groupId))
            {
                List<string> atNames = new List<string>();

                foreach(TypeValueData tv in infos)
                {
                    if (tv.type == "at")
                    {
                        atNames.Add(tv.value.Replace("\u2005", ""));
                    }
                }

                UserNameInfo myNamesInGroup = this.GetGroupMemberNames(msgInfo.user.groupId, this.myAccount.UserName);

                if (myNamesInGroup == null)
                {
                    myNamesInGroup = this.GetContactNames(this.myAccount.UserName);
                }

                if (myNamesInGroup != null)
                {
                    if (!string.IsNullOrWhiteSpace(myNamesInGroup.RemarkName) && atNames.Contains(myNamesInGroup.RemarkName))
                    {
                        msgInfo.user.isAtMe = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(myNamesInGroup.NickName) && atNames.Contains(myNamesInGroup.NickName))
                    {
                        msgInfo.user.isAtMe = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(myNamesInGroup.DisplayName) && atNames.Contains(myNamesInGroup.DisplayName))
                    {
                        msgInfo.user.isAtMe = true;
                    }
                }
            }

            str_msg_all = str_msg_all.Replace("\u2005", "");
            str_msg = str_msg.Replace("\u2005", "");
            string infoStr = JsonConvert.SerializeObject(infos);

            string[] results = new string[3];
            results[0] = str_msg_all;
            results[1] = str_msg;
            results[2] = infoStr;

            return results;            
        }
        /*
         * """
        msg_content.type:
            0 -> Text
            1 -> Location
            3 -> Image
            4 -> Voice
            5 -> Recommend
            6 -> Animation
            7 -> Share
            8 -> Video
            9 -> VideoCall
            10 -> Redraw
            11 -> Empty
            99 -> Unknown
        :param msg_type_id: 消息类型id
        :param msg: 消息结构体
        :return: 解析的消息
        """        
         */
        private MessageContent ExtractContent(ref MessageInfo msgInfo, int msg_type_id, dynamic msg)
        {
            MessageContent msg_content = new MessageContent();
            int mtype = (int)msg.MsgType;
            string content = WebUtility.HtmlDecode(msg.Content.ToString());
            string msg_id = msg.MsgId;

            switch(msg_type_id)
            {
                case 0:
                    msg_content.type = 11;
                    msg_content.data = string.Empty;
                    return msg_content;
                case 2:
                    msg_content.type = 0;
                    msg_content.data = content.Replace("<br/>", "\n");
                    return msg_content;                    
                case 3:
                    string[] tmps = content.Split(new string[] { "<br/>" }, StringSplitOptions.None);                        
                    string uid = "";
                    if (tmps.Length > 1)
                    {
                        uid = tmps[0];
                        uid = uid.Substring(0, uid.Length - 1);
                        content = "";
                        for (int i = 1; i < tmps.Length; i ++)
                        {
                            if (i > 1)
                            {
                                content += "\n";
                            }
                            content += tmps[i];
                        }

                        string gid = msg.FromUserName.ToString();

                        string name = GetPerferName(this.GetContactNames(uid));

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = GetPerferName(this.GetGroupMemberNames(gid, uid));
                        }

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = "unknown";
                        }

                        MsgSourceInfo user = new MsgSourceInfo();
                        user.senderId = uid;
                        user.senderName = name;
                        user.groupId = gid;                        

                        msgInfo.user = user;                        
                    }
                    break;
                default:
                    break;
            }

            switch(mtype)
            {
                case 1:
                    if (content.Contains("http://weixin.qq.com/cgi-bin/redirectforward?args="))
                    {
                        string result = this.wechatClient.GetString(content);
                        string pos = SearchContent("title", result, "xml");
                        msg_content.type = 1;
                        msg_content.data = pos;
                        msg_content.detail = result;
                    }
                    else
                    {
                        msg_content.type = 0;

                        if (msg_type_id == 3 || (msg_type_id == 1 && msg.ToUserName.ToString().StartsWith("@@")))
                        {
                            string[] msg_infos = this.ProcAtInfo(ref msgInfo, content);
                            msg_content.data = msg_infos[0];
                            msg_content.detail = msg_infos[2];
                            msg_content.desc = msg_infos[1];
                        }
                        else
                        {
                            msg_content.data = content;
                        }
                    }
                    break;
                case 3:                    
                    msg_content.type = 3;
                    msg_content.data = this.baseURI + "/webwxgetmsgimg?MsgID=" + msg_id + "&skey=" + this.skey;
                    string res = this.wechatClient.GetString(msg_content.data);
                    byte[] byteArray = UnicodeEncoding.UTF8.GetBytes(res);
                    msg_content.img = byteArray;
                    break;
                case 34:                    
                    msg_content.type = 4;
                    msg_content.data = this.baseURI + "/webwxgetvoice?msgid=" + msg_id + "&skey=" + this.skey;
                    string voice = this.wechatClient.GetString(msg_content.data);
                    byte[] vArray = UnicodeEncoding.UTF8.GetBytes(voice);
                    msg_content.voice = vArray;
                    break;
                case 37:                    
                    msg_content.type = 37;
                    msg_content.data = msg.RecommendInfo.ToString();
                    break;
                case 42:                   
                    msg_content.type = 5;
                    dynamic info = msg.RecommendInfo;

                    Dictionary<string,string> data = new Dictionary<string, string>();
                    data.Add("nickname", info.NickName.ToString());
                    data.Add("alias", info.Alias.ToString());
                    data.Add("province", info.Province.ToString());
                    data.Add("city", info.City.ToString());
                    data.Add("gender", info.Sex.ToString());

                    msg_content.data = JsonConvert.SerializeObject(data);
                    break;
                case 47:                    
                    msg_content.type = 6;
                    msg_content.data = this.SearchContent("cdnurl", content, null);
                    break;
                case 49:                    
                    msg_content.type = 7;

                    int appType = -1;
                    try
                    { 
                        string appTypeStr = msg.AppMsgType.ToString();
                        appType = int.Parse(appTypeStr);
                    }
                    catch(Exception e)
                    {
                        log.Error(e.Message);
                    }

                    string app_msg_type = "unknown";
                    switch (appType)
                    {
                        case 3:
                            app_msg_type = "music";
                            break;
                        case 5:
                            app_msg_type = "link";
                            break;
                        case 7:
                            app_msg_type = "weibo";
                            break;
                        default:
                            break;
                    }

                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add("type", app_msg_type);
                    dict.Add("title", msg.FileName.ToString());
                    dict.Add("desc", this.SearchContent("des", content, "xml"));
                    dict.Add("url", msg.Url);
                    dict.Add("from", this.SearchContent("appname", content, "xml"));
                    dict.Add("content", msg.Get("Content"));

                    msg_content.data = JsonConvert.SerializeObject(dict);

                    break;             
                case 62:
                    msg_content.type = 8;
                    msg_content.data = content;
                    break;
                case 53:
                    msg_content.type = 9;
                    msg_content.data = content;
                    break;
                case 10002:
                    msg_content.type = 10;
                    msg_content.data = content;
                    break;
                case 10000:
                    msg_content.type = 12;
                    msg_content.data = content;
                    break;
                case 43:
                    msg_content.type = 13;
                    msg_content.data = this.baseURI + "/webwxgetvideo?msgid=" + msg_id + "&skey=" + this.skey; 
                    break;
                default:
                    msg_content.type = 99;
                    msg_content.data = content;
                    break;

            }
                        
            return msg_content;
        }

        public void HandleMessage(string result)
        {           
            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);
            foreach (var msg in json.AddMsgList)
            {
                MsgSourceInfo user = new MsgSourceInfo();
                user.senderId = msg.FromUserName;
                user.senderName = "unknown";

                int msgTypeId = 99;

                if ((int)msg.MsgType == 51 && (int)msg.StatusNotifyCode == 4)
                {
                    msgTypeId = 0;
                    user.senderName = "system";

                    ///TODO 写入文件
                }
                else if ((int)msg.MsgType == 37) /// Friend Request
                {
                    msgTypeId = 37;

                    return;
                }
                else if (msg.FromUserName.ToString() == this.myAccount.UserName)
                {
                    msgTypeId = 1;
                    user.senderName = "self";
                }
                else if (msg.ToUserName.ToString() == "filehelper")
                {
                    msgTypeId = 2;
                    user.senderName = "file_helper";
                }
                else if ((msg.FromUserName.ToString()).StartsWith("@@"))
                {
                    msgTypeId = 3;
                    user.senderName = GetPerferName(GetContactNames(user.senderId));                    

                    if (user.senderName == null)
                    {
                        user.senderName = "unknown";
                    }
                }
                else
                {
                    ContactInfoWithType it = userMgmtStore.GetNormalAccountInfo(user.senderId);//this.normalAccountInfo[user.senderId];
                    if (it != null)
                    {
                        switch (it.type)
                        {
                            case MessageType.Contact:
                                msgTypeId = 4;
                                user.senderName = GetPerferName(GetContactNames(user.senderId));
                                break;
                            case MessageType.Public:
                                msgTypeId = 5;
                                user.senderName = GetPerferName(GetContactNames(user.senderId));
                                break;
                            case MessageType.Special:
                                msgTypeId = 6;
                                user.senderName = GetPerferName(GetContactNames(user.senderId));
                                break;
                            default:
                                break;
                        }
                    }
                   /* else /// Added by Julia: when the contact is not stored locally, msgTypeId = 100
                    {
                        msgTypeId = 100;                        
                    }
                    */
                }

                if (string.IsNullOrWhiteSpace(user.senderName))
                {
                    user.senderName = "unknown";
                }

                user.senderName = WebUtility.HtmlDecode(user.senderName);

                MessageInfo message = new MessageInfo();
                message.user = user;
                message.msg_type_id = msgTypeId;
                message.msg_id = msg.MsgId.ToString();
                message.to_user_id = msg.ToUserName.ToString();
                

                MessageContent content = this.ExtractContent(ref message, msgTypeId, msg);
                message.content = content;

                MessageHandler msgHandler = new MessageHandler();
                ReplyInfo rinfo = msgHandler.HandleMessageAll(message);

                if (!string.IsNullOrWhiteSpace(rinfo.reply))
                { 
                    this.SendMessageByUID(rinfo.reply, msg.FromUserName.ToString());
                }

            }
            
        }

        private string GenerateMessageId()
        {
            string str = DateTime.Now.ToFileTime().ToString().Substring(0, 12);
            Random random = new Random();
            for(int i = 0; i < 5; i ++)
            { 
                str += random.Next(10).ToString();
            }
            return str;
        }

        private string ToUnicode(string word)
        {
            return word;
        }
        
        class ResponseMessage
        {
            public int Type;
            public string Content;
            public string FromUserName;
            public string ToUserName;
            public string LocalID;
            public string ClientMsgId;

        }
        class ResponseParam
        {
            public BaseRequest BaseRequest;
            public ResponseMessage Msg;
        }

        private bool SendMessageByUID(string word, string dst)
        {
            
            if (string.IsNullOrWhiteSpace(dst))
            {
                dst = "filehelper";
            }

            log.Info("Replay to (" + dst + ") msg: " + word);

            string url = this.baseURI + "/webwxsendmsg?pass_ticket=" + this.pass_ticket;

            string msgId = GenerateMessageId();

            word = ToUnicode(word);

            ResponseMessage msg = new ResponseMessage();
            msg.Type = 1;
            msg.Content = word;
            msg.FromUserName = this.myAccount.UserName;
            msg.ToUserName = dst;
            msg.LocalID = msgId;
            msg.ClientMsgId = msgId;

            ResponseParam param = new ResponseParam();
            param.BaseRequest = this.baseRequest;
            param.Msg = msg;

            string paramStr = JsonConvert.SerializeObject(param);
            string result = this.wechatClient.Post(url, paramStr);

            dynamic json = JsonConvert.DeserializeObject<dynamic>(result);

            int ret = json.BaseResponse.Ret;
            
            if (ret == 0)
            {
                log.Info("Sent response to msg sender successfully.");
                return true;
            }
            else
            {
                log.Error("Failed to send response to msg sender.");
                return false;
            }
        }

        private string GetPerferName(UserNameInfo unInfo)
        {
            if (unInfo == null)
            {
                return null;
            }    
                               
            if (!string.IsNullOrWhiteSpace(unInfo.RemarkName))
            {
                return unInfo.RemarkName;
            }
           else if (!string.IsNullOrWhiteSpace(unInfo.NickName))
            {
                return unInfo.NickName;
            }
           else if (!string.IsNullOrWhiteSpace(unInfo.DisplayName))
            {
                return unInfo.DisplayName;
            }
            return null;               
        }    
        
        private UserNameInfo GetContactNames(string userId)
        {
            //if (!this.normalAccountInfo.Keys.Contains(userId))
            //{
            //    return null;
            //}
            ContactInfoWithType info = userMgmtStore.GetNormalAccountInfo(userId);

            if (info == null)
            {
                return null;
            }

            UserNameInfo unInfo = new UserNameInfo();
            //ContactInfoWithType info = this.normalAccountInfo[userId];

            if (!string.IsNullOrWhiteSpace(info.info.RemarkName))
            {
                unInfo.RemarkName = info.info.RemarkName;
            }

            if (!string.IsNullOrWhiteSpace(info.info.NickName))
            {
                unInfo.NickName = info.info.NickName;
            }

            if (!string.IsNullOrWhiteSpace(info.info.DisplayName))
            {
                unInfo.DisplayName = info.info.DisplayName;
            }

            return unInfo;
        }

        private UserNameInfo GetGroupMemberNames(string groupId, string userId)
        {
            //if (!this.groupMembers.Keys.Contains(groupId))
            //{
            //    return null;
            //}

            Dictionary<string, ContactInfo> aGroupMembers = userMgmtStore.GetGroupMembers(groupId);//this.groupMembers[groupId];

            if (aGroupMembers == null)
            {
                return null;
            }

            if (aGroupMembers != null)
            {
                if (!aGroupMembers.Keys.Contains(userId))
                {
                    return null;
                }

                if (aGroupMembers[userId] != null)
                {
                    ContactInfo member = aGroupMembers[userId];
                    UserNameInfo unInfo = new UserNameInfo();                    

                    if (!string.IsNullOrWhiteSpace(member.RemarkName))
                    {
                        unInfo.RemarkName = member.RemarkName;
                    }

                    if (!string.IsNullOrWhiteSpace(member.NickName))
                    {
                        unInfo.NickName = member.NickName;
                    }

                    if (!string.IsNullOrWhiteSpace(member.DisplayName))
                    {
                        unInfo.DisplayName = member.DisplayName;
                    }

                    return unInfo;

                }
                return null;
            }

            return null;
        }

        public void ProcMessage()
        {
            log.Info("Start to process messages.");

            this.TestSyncCheck();
            this.status = "loginsuccess";

            DateTime checkTime;
            while (true)
            {
                if (this.status == "wait4loginout")
                    return;

                checkTime = DateTime.Now;

                try
                {
                    SyncInfo syncInfo = this.SyncCheck();
                    switch (syncInfo.retcode)
                    {
                        case "1100":
                            //log.Info("retcode is 1100.");
                            break;
                        case "1101":
                            //log.Info("retcode is 1101.");
                            break;
                        case "0":
                            log.Info("retcode is 0, selector is " + syncInfo.selector);                         
                            string r = null;
                            switch (syncInfo.selector) { 
                                case "2":
                                case "3":
                                case "4":
                                case "7":
                                    r = this.Sync();
                                    if (r != null)
                                    {
                                        this.HandleMessage(r);
                                    }
                                    break;                                                                  
                                case "6":
                                    r = this.Sync();
                                    if (r != null)
                                    {
                                        this.GetContacts();
                                    }
                                    break;                                                                    
                                case "0":
                                    break;
                                default:
                                    log.Debug("[DEBUG] sync_check:" + syncInfo.retcode + ", " + syncInfo.selector);
                                    r = this.Sync();
                                    if (r != null)
                                    {
                                        this.HandleMessage(r);
                                    }
                                    break;
                            }
                            break;
                        default:
                            log.Debug("Begin to sleep 10 seconds.");
                            Thread.Sleep(10 * 1000);
                            break;
                    }
                    this.Schedule();
                }
                catch(Exception e)
                {
                    log.Error(e.Message);
                }

                DateTime nowTime = DateTime.Now;
                double latency = nowTime.Subtract(checkTime).TotalSeconds;
                if (latency < 0.8)
                {
                    log.Debug("Begin to sleep " + (1.0 - latency).ToString() + " seconds.");
                    Thread.Sleep( (int) ((1.0 - latency) * 1000));
                }
            }           
        }

        public void Run()
        {
            try
            {
                this.GetUUID();
                this.GenerateQRCode();
                this.ShowWechatLoginQR();

                string code = this.WaitForLogin();

                if (code != SUCCESS)
                {
                    log.Error("[ERROR] Web Wechat login failed. failed code = " + code);
                    this.status = "loginout";
                    return;
                }

                if (this.Login())
                {
                    log.Info("[INFO] Web Wechat login succeeded.");
                }
                else
                {
                    log.Error("[ERROR] Web Wechat login failed.");
                    this.status = "loginout";
                    return;
                }

                if (this.Init())
                {
                    log.Info("[INFO] Web Wechat init succeeded.");
                }
                else
                {
                    log.Error("[ERROR] Web Wechat init failed.");
                    this.status = "loginout";
                    return;
                }

                this.NotifyStatus();

                if (this.GetContacts())
                {
                    log.Info("[INFO] Web Wechat get contacts succeefully.");
                }

                if (this.BatchGetGroupMembers())
                {
                    log.Info("[INFO] Web Wechat get group members succeefully.");
                }

                this.ProcMessage();
                this.status = "loginout";
            }
            catch (Exception e)
            {
                log.Error("[ERROR]: Web Wechat run failed. " + e.Message);
                this.status = "loginout";
            }
        }

        public static void Main(string[] args)
        {
            WechatProxy proxy = new WechatProxy();
            proxy.Run();
            
            log.Info("Finished!");
        }
    }
}
