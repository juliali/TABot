using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA.Engine.Data;
using TA.Engine.Common;
using Newtonsoft.Json;

namespace TA.Engine.Controllers.Component
{
    public class MemContextManager : IContextManager
    {
        private static ContextStore cStore = ContextStore.Instance;        

        private ContextInfo Wrap(BotContext context)
        {
            ContextInfo ci = new ContextInfo();
            ci.type = context.type;
            ci.lastUpdatedTime = DateTime.Now;
            ci.jsonString = JsonConvert.SerializeObject(context);

            return ci;
        }
        
        public void StoreESContext(ExamSuitContext esContext, string userId)
        {
            //throw new NotImplementedException();
            ContextInfo ci = Wrap(esContext);
            cStore.SetContextInfo(StoreType.ESOnly, ci, userId);
        }

        public ContextInfo GetAndRemoveESContext(string userId)
        {
            ContextInfo ci = cStore.GetContextInfo(StoreType.ESOnly, userId);
            if (ci != null)
            {
                cStore.DeleteContextInfo(StoreType.ESOnly, userId);
            }
            return ci;
        }

        public void CreateContext(BotContext context, string userId)
        {
            ContextInfo ci = Wrap(context);
            cStore.SetContextInfo(StoreType.AllType, ci, userId);
        }

        public ContextInfo GetContext(string userId)
        {            
            return cStore.GetContextInfo(StoreType.AllType, userId);
        }

        public void RemoveContext(string userId)
        {
            cStore.DeleteContextInfo(StoreType.AllType, userId);
        }
       
        public void UpdateContext(BotContext context, string userId)
        {
            ContextInfo ci = Wrap(context);
            cStore.SetContextInfo(StoreType.AllType, ci, userId);
        }
    }
}
