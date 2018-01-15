using TA.Engine.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Controllers.Component
{
    public interface IContextManager
    {
        void StoreESContext(ExamSuitContext esContext, string userId);
        ContextInfo GetAndRemoveESContext(string userId);

        void CreateContext(BotContext context, string userId);
        void UpdateContext(BotContext context, string userId);

        ContextInfo GetContext(string userId);

        void RemoveContext(string userId);
    }
}
