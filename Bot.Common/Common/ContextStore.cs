using System;
using System.Collections.Generic;
using Bot.Common.Data;

namespace Bot.Common.Common
{
    public class ContextStore
    {
        private static ContextStore instance;

        private static Dictionary<string, AbstractContext> contextMap;

        private ContextStore()
        {
            contextMap = new Dictionary<string, AbstractContext>();
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

        public AbstractContext GetContext(string userId, BotType type)
        {
            // If userId is null, set a uniq id for each uatterance.
            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = DateTime.Now.Ticks.ToString(); 
            }

            if (contextMap.ContainsKey(userId))
            {
                return contextMap[userId];
            }
            else
            {
                AbstractContext newContext = AbstractContext.GetAbstractContext(userId, type);
                contextMap.Add(userId, newContext);

                return newContext;
            }
        } 
        
        public void SetContext(string userId, AbstractContext context)
        {
            contextMap[userId] = context;
        }      
    }
}
