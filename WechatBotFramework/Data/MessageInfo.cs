using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WechatBotFramework.Data
{
    public class ReplyInfo
    {
        public string reply;
        //public string toUserId;
    }

    public class MessageInfo
    {
        public int msg_type_id;
        public string msg_id;
        public MessageContent content;
        public string to_user_id;
        public MsgSourceInfo user;
    }

    public class MessageContent
    {
        public int type;
        public string data;
        public string detail;
        public string desc;
        public byte[] img;
        public byte[] voice;
    }

    public class MsgSourceInfo
    {
        public string senderId;
        public string senderName;
        public string groupId;
        public bool isAtMe = false;
    }

  /*  public class MessageContentDataInfo
    {
        public string nickname;
        public string alias;
        public string province;
        public string city;
        public string gender;
    }*/
}
