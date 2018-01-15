using Bot.Common.Data;
using Bot.Common.LUEngine.Luis;
using Bot.Common.Utils;
using Newtonsoft.Json;
using TA.Engine.Common;
using TA.Engine.Controllers.Component;
using TA.Engine.Data;
using TA.Engine.Engines;
using TA.Engine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Controllers
{
    public class DialogController
    {
        private readonly HashSet<string> ValidPSAIntents = new HashSet<string> {"AskTest", "AskExam", "AskCourseSchedule", "AskDelivery", "AskTrainingSchedule", "AskHomework", "AskNotification", "AskStoryContribution", "AskStorySchedule", "Greeting", "Praise", "AskSwimmingClass", "None" };
        public readonly string ONEONEPREFIX = "SINGLE_CONTACT";

        private LuisClient luisClient = new LuisClient();

        private static RuleTextStore ruleStore = RuleTextStore.Instance;
        private IContextManager cManager = new MemContextManager();
            //new DBContextManager();

        public LUInfo Understand(string utterance, bool isSingleContact)
        {
            LUInfo currentLUInfo = null;

            if (string.IsNullOrWhiteSpace(utterance))
            {                
                currentLUInfo = new LUInfo();
                currentLUInfo.Intent = new Intent();
                currentLUInfo.Intent.intent = "Greeting";
                currentLUInfo.EntityList = new List<Entity>();
            }
            else
            {
                utterance = ruleStore.Preprocess(utterance);

                string ruleBasedIntent = ruleStore.DetermineIntent(utterance);               
                
                if ((!isSingleContact) && (!string.IsNullOrWhiteSpace(ruleBasedIntent)) &&　ruleBasedIntent == "DoTest")
                {
                    ruleBasedIntent = null;
                }

                if (!string.IsNullOrWhiteSpace(ruleBasedIntent))
                {
                    if (ruleBasedIntent == "DoTest") 
                    {
                        currentLUInfo = new TestLUInfo();                        
                    }
                    else if (ruleBasedIntent == "DoDISC")
                    {
                        currentLUInfo = new ExamLUInfo();
                    }
                    else
                    {
                        currentLUInfo = new LUInfo();
                    }

                    currentLUInfo.Intent.intent = ruleBasedIntent;
                    currentLUInfo.Intent.score = 1;
                    currentLUInfo.EntityList = new List<Entity>();

                }

                if (currentLUInfo == null)                
                { 
                    currentLUInfo = this.luisClient.Query(utterance);
                }

                List<Entity> rulebasedEntities = ruleStore.ExtractSlot(utterance);
                if (rulebasedEntities.Count > 0)
                {
                    currentLUInfo.EntityList.AddRange(rulebasedEntities);
                }
            }
            
            return currentLUInfo;
        }

        private PSAContext initPSAContext(string userId, LUInfo luinfo)
        {
            PSAContext context = new PSAContext(userId);

            string intent = luinfo.Intent.intent;

            string coursename = DatetimeUtils.GetCourseName(luinfo);
            TimeRange range = DatetimeUtils.GetTimeRange(luinfo);

            context.Intent = intent;
            context.CourseName = coursename;
            context.timeRange = range;

            context.validTime = DateTime.Now;

            if (context.timeRange == null)
            {
                context.timeRange = new TimeRange();
                DateTime now = DateTime.Now;
                context.timeRange.startDate = now;
                context.timeRange.endDate = now;
            }

            return context;
        }

        private PSAContext updatePSAContext(ref PSAContext context, LUInfo luinfo)
        {
            string intent = luinfo.Intent.intent;

            if (!string.IsNullOrWhiteSpace(intent) && ValidPSAIntents.Contains(intent))
            {
                context.Intent = intent;
            }

            string cn = DatetimeUtils.GetCourseName(luinfo);

            if (!string.IsNullOrWhiteSpace(cn))
            {
                context.CourseName = cn;
            }

            TimeRange range = DatetimeUtils.GetTimeRange(luinfo);

            if (range != null)
            {
                context.timeRange = range;
            }

            return context;
        }

        private TaskFlowContext updateTFContext(ref TaskFlowContext context, string utterance)
        {
            context.IsInTesting = true;
            context.UserInput = utterance;            

            return context;
        }

        private ExamSuitContext updateESContext(ref ExamSuitContext context, string utterance)
        {                             
            context.UserInput = utterance;
                       
            return context;
        }

        private TaskFlowContext InitTFContext(string userId, TestLUInfo currentLUInfo)
        {
            TaskFlowContext tfContext = new TaskFlowContext(userId);
                   
            tfContext.Intent = currentLUInfo.Intent.intent; 
            tfContext.CourseName = DatetimeUtils.GetCourseName(currentLUInfo);
            tfContext.currentIndex = 0;

            return tfContext;
        }

        private ExamSuitContext InitESContext(string userId, ExamLUInfo currentLUInfo)
        {
            ExamSuitContext esContext = new ExamSuitContext(userId);
            esContext.Intent = currentLUInfo.Intent.intent;
            esContext.currentIndex = 0;

            return esContext;                                 
        }

        private bool IsValid(PSAContext context)
        {

            DateTime nowTime = DateTime.Now;

            if (context.validTime == null)
            {
                return false;
            }
            else if (nowTime.Subtract(context.validTime).TotalMinutes > 30)
            {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(context.CourseName))
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        private ExamSuitContext GetCachedESContext(string userId)
        {
            ContextInfo ci = cManager.GetAndRemoveESContext(userId);
            if (ci == null)
            {
                return null;
            }

            DateTime currentTime = DateTime.Now;
            if (currentTime.Subtract(ci.lastUpdatedTime).TotalMinutes > 480) //8 hours
            {
                return null;
            }
            else
            {
                ExamSuitContext esContext = JsonConvert.DeserializeObject<ExamSuitContext>(ci.jsonString);
                return esContext;
            }
        }

        private BotContext GetValidContext(string userId)
        {
            ContextInfo ci = cManager.GetContext(userId);
            if (ci == null)
            {
                return null;
            }

            DateTime currentTime = DateTime.Now;
            if (currentTime.Subtract(ci.lastUpdatedTime).TotalMinutes > 480) // 8 hours
            {
                return null;
            }
            else
            {
                if (ci.type == ContextType.PSAContext)
                { 
                    PSAContext psaContext = JsonConvert.DeserializeObject<PSAContext>(ci.jsonString);

                    if (IsValid(psaContext))
                    {
                        return psaContext;
                    }
                    else
                    {
                        return null;
                    }
                    
                }
                else if (ci.type == ContextType.ExamSuitContext)
                {
                    ExamSuitContext esContext = JsonConvert.DeserializeObject<ExamSuitContext>(ci.jsonString);
                    return esContext;
                }
                else
                {
                    TaskFlowContext tfContext = JsonConvert.DeserializeObject<TaskFlowContext>(ci.jsonString);
                    return tfContext;
                }                
            }
        }

        private bool IsSingleContact(ref string utterance)
        {
            if (string.IsNullOrWhiteSpace(utterance))
            {
                return false;
            }

            bool isSingleContact = false;

            if (utterance.StartsWith(ONEONEPREFIX))
            {
                isSingleContact = true;
            }

            utterance = utterance.Replace(ONEONEPREFIX, "");
            return isSingleContact;
        }

        public string Answer(string userId, string utterance)
        {            
            BotContext context = this.GetValidContext(userId);

            bool isSingleContact = this.IsSingleContact(ref utterance);

            if (context == null)
            {                
                LUInfo luInfo = Understand(utterance, isSingleContact);
                if (luInfo.GetType() == typeof(TestLUInfo))
                {
                    context = InitTFContext(userId, (TestLUInfo)luInfo);
                }
                else if (luInfo.GetType() == typeof(ExamLUInfo))
                {
                    context = InitESContext(userId, (ExamLUInfo)luInfo);
                }
                else
                {
                    context = initPSAContext(userId, luInfo);
                }

                cManager.CreateContext(context, userId);
            }
            else
            {                
                if (context.type == ContextType.TaskFlowContext && isSingleContact)
                {
                    TaskFlowContext tfContext = (TaskFlowContext)context;
                    context = updateTFContext(ref tfContext, utterance);                    
                }
                else if (context.type == ContextType.ExamSuitContext && isSingleContact)
                {                     
                    ExamSuitContext esContext = (ExamSuitContext)context;
                    context = updateESContext(ref esContext, utterance);                    
                }
                else
                {
                    LUInfo luInfo = Understand(utterance, isSingleContact);

                    if (context.type == ContextType.PSAContext)
                    {                        
                        PSAContext psacontext = (PSAContext)context;
                        context = updatePSAContext(ref psacontext, luInfo);
                    }
                    else
                    {                        
                        return "[Error]: Unknown Context Type.";
                    }
                }                
            }

            string answer = null;
            switch(context.type)
            {
                case ContextType.PSAContext:
                    ChatTableEngine engine = new ChatTableEngine();
                    PSAContext psacontext = (PSAContext)context;

                    answer = engine.Answer(userId, ref psacontext);

                    cManager.UpdateContext(psacontext, userId);
                    break;
                case ContextType.TaskFlowContext:
                    TestEngine engineT = new TestEngine();
                    TaskFlowContext tfContext = (TaskFlowContext)context;

                    answer = engineT.Answer(userId, ref tfContext);

                    if (tfContext.IsInTesting)
                    { 
                        cManager.UpdateContext(tfContext, userId);
                    }
                    else
                    {
                        cManager.RemoveContext(userId);
                    }
                    break;
                case ContextType.ExamSuitContext:
                    
                    ExamSuitContext esContext = (ExamSuitContext)context;
                    DICSExamSrv taskSrv = new DICSExamSrv();
                    
                    string userInput = esContext.UserInput;

                    switch(esContext.status)
                    {
                        case ESStatus.Started:
                            ExamSuitContext cachedContext = this.GetCachedESContext(userId);
                            answer = taskSrv.StartTest(ref esContext, cachedContext);
                            break;

                        case ESStatus.Restarted:
                            answer = taskSrv.ContinueOrRefresh(ref esContext, userInput);
                            break;

                        case ESStatus.OnGoing:
                            answer = taskSrv.ReceiveUserAnswer(ref esContext, userInput);
                            break;
                            
                    }

                    if (esContext.status == ESStatus.Finished || esContext.status == ESStatus.Aborded)
                    {
                        cManager.RemoveContext(userId);
                    }
                    else if (esContext.status == ESStatus.Paused)
                    {
                        cManager.StoreESContext(esContext, userId);
                        cManager.RemoveContext(userId);
                    }
                    else
                    {
                        cManager.UpdateContext(esContext, userId);
                    }
                  
                    break;
            }
               
            return answer;
        }

    }
}
