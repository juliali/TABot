using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WechatBotFramework.Data
{
    
    public class GroupInfoWithType : ContactInfoWithType
    {
        public dynamic group;        
    }
    public class ContactInfoWithType
    {
        public MessageType type;
        public ContactInfo info;
    }
    public class ContactInfo
    {
        public int Uin;
        public string UserName;
        public string NickName;

        public string HeadImgUrl;
        public int ContactFlag;
	    public int MemberCount;
	    public string[] MemberList;
        public string RemarkName;
        public int HideInputBarFlag;
        public int Sex;
        public string Signature;
	    public int VerifyFlag;
        public int OwnerUin;
        public string PYInitial;
        public string PYQuanPin;
        public string RemarkPYInitial;
        public string RemarkPYQuanPin;
	    public int StarFriend;
        public int AppAccountFlag;

        public int Statues;

        public long AttrStatus;

        public string Province;
        public string City;
        public string Alias;

        public int SnsFlag;

        public int UniFriend;
        public string DisplayName;

        public int ChatRoomId;
        public string KeyWord;
        public string EncryChatRoomId;

        public int IsOwner;

}
    public class KeyVal
    {
        public long Key;
        public long Val;
    }

    public class SyncKeyInfo
    {
        public int Count;
        public List<KeyVal> List;
    }
}
