using Bot.Common.Data;
using TA.Engine.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Services
{
    
        public abstract class IntentSrv
        {
            public abstract string GetAnswer(PSAContext context);
        }
    
}
