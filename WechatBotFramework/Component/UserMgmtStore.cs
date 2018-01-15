using System.Collections.Generic;
using WechatBotFramework.Data;

namespace WechatBotFramework.Component
{
    public class UserMgmtStore
    {
        private static UserMgmtStore instance;

        private Dictionary<string, Dictionary<string, ContactInfo>> groupMembers;
        private Dictionary<string, ContactInfoWithType> normalAccountInfo;
        private List<ContactInfo> groupList;

        private UserMgmtStore()
        {
            groupList = new List<ContactInfo>();
            groupMembers = new Dictionary<string, Dictionary<string, ContactInfo>>();        
            normalAccountInfo = new Dictionary<string, ContactInfoWithType>();            
        }

        public ContactInfoWithType GetNormalAccountInfo(string key)
        {
            if (!this.normalAccountInfo.ContainsKey(key))
            {
                return null;
            }

            return this.normalAccountInfo[key];
        }

        public void SetNoramlAccountInfo(string key, ContactInfoWithType value)
        {
            if (!this.normalAccountInfo.ContainsKey(key))
            {
                this.normalAccountInfo.Add(key, value);
            }
            else
            {
                this.normalAccountInfo[key] = value;
            }
        }

        public Dictionary<string, ContactInfo> GetGroupMembers(string key)
        {
            if (!this.groupMembers.ContainsKey(key))
            {
                return null;
            }

            return this.groupMembers[key];
        }

        public void SetGroupMembers(string key, Dictionary<string, ContactInfo> members)
        {
            if (!this.groupMembers.ContainsKey(key))
            {
                this.groupMembers.Add(key, members);
            }
            else
            {
                this.groupMembers[key] = members;
            }
        }

        public void AddGroup(ContactInfo group)
        {
            this.groupList.Add(group);
        }

        public ContactInfo GetGroup(int index)
        {
            if (index >= this.groupList.Count)
            {
                return null;
            }


            return this.groupList[index];
        }

        public int GetGroupNumber()
        {
            return this.groupList.Count;
        }

        public static UserMgmtStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserMgmtStore();
                }
                return instance;
            }
        }
    }
}
