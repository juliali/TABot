using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public abstract class BotContext
    {
        public ContextType type;

        public string userId;
        public string Intent;
        
        public DateTime validTime;
        public string CourseName;

        public string GetUserId()
        {
            return userId;
        }
    }
}
