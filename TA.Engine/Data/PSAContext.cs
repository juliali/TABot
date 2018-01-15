using Bot.Common.Data;
using Bot.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public class PSAContext : BotContext
    {
                    
        public TimeRange timeRange;        
        
        public PSAContext(string userId)
        {
            this.userId = userId;
            this.type = ContextType.PSAContext;
        }        
    }
}

