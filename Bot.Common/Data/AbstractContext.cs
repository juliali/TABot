using Bot.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Common.Data
{
    public enum BotType
    {
        WB, PSA
    }
    public abstract class AbstractContext
    {
        public DateTime validTime;
        public string userId;

        public string Intent;
        
        public TimeRange timeRange;

        public static AbstractContext GetAbstractContext(string userId, BotType type)
        {
            
            if (type == BotType.WB)
            { 
            return new WBContext(userId);
            }
            else
            {
                return null; //new PSAContext(userId);
            }
        }       
    }
}

