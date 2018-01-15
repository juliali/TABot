using TA.Engine.Services;
using TA.Engine.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Common
{    
    public enum StoreType
    {
        AllType, ESOnly
    }

    public class ContextStore
    {
        private static ContextStore instance;

        private static Dictionary<StoreType, Dictionary<string, ContextInfo>> ContextMap;
        

        private ContextStore()
        {
            ContextMap = new Dictionary<StoreType, Dictionary<string, ContextInfo>>();
            ContextMap.Add(StoreType.AllType, new Dictionary<string, ContextInfo>());
            ContextMap.Add(StoreType.ESOnly, new Dictionary<string, ContextInfo>());
        }

        public static ContextStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ContextStore();
                }
                return instance;
            }
        }

        public ContextInfo GetContextInfo(StoreType st, string userId)
        {
            // If userId is null, set a uniq id for each uatterance.
            if (string.IsNullOrWhiteSpace(userId))
            {                
                throw new Exception("userId is required for task servers.");
            }

            if (ContextMap[st].ContainsKey(userId))
            {
                return ContextMap[st][userId];
            }
            else
            {
                return null;
            }
        }

        public void SetContextInfo(StoreType st, ContextInfo context, string userId)
        {
            // If userId is null, set a uniq id for each uatterance.
            if (string.IsNullOrWhiteSpace(userId))
            {
                //userId = DateTime.Now.Ticks.ToString();
                throw new Exception("userId is required for task servers.");
            }

            if (ContextMap[st].ContainsKey(userId))
            {
                ContextMap[st][userId] = context;
            }
            else
            {
                ContextMap[st].Add(userId, context);
            }
        }

        public void DeleteContextInfo(StoreType st, string userId)
        {
            // If userId is null, set a uniq id for each uatterance.
            if (string.IsNullOrWhiteSpace(userId))
            {
                //userId = DateTime.Now.Ticks.ToString();
                throw new Exception("userId is required for task servers.");
            }

            if (ContextMap[st].ContainsKey(userId))
            {
                ContextMap[st].Remove(userId);
            }
        }
    }
}
