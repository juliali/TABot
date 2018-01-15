using System;
using System.Collections.Generic;

using Bot.Common.Common;
using Bot.Common.Data;
using Bot.Common.LUEngines;
using Bot.Common.Utils;
using TA.Engine.Data;
using Newtonsoft.Json;
using TA.Engine.Controllers.Component;
using System.Text.RegularExpressions;
using TA.Engine.Services;


namespace TA.Engine.Controllers
{    
    public class IntentController
    {       
        private List<IntentSrv> srvs;
        private CalculatorSrv taskSrv = new CalculatorSrv();
        private LUController luController = new LUController();
        private ContextStore contextStore = ContextStore.Instance;
       // private TaskServerStore srvStore = TaskServerStore.Instance;

        private readonly HashSet<string> ValidIntent = new HashSet<string> { "AskCourseSchedule", "AskDelivery", "AskTrainingSchedule", "AskHomework", "AskNotification", "AskStoryContribution", "AskStorySchedule", "Greeting", "Praise", "AskSwimmingClass", "None" };
        //"Weather", "Smog", "DefaultIntent", "CarWashing", "RestrictedDriving", "Cloth" };

        public readonly string ONEONEPREFIX = "SINGLE_CONTACT";
        private readonly string DOTESTINTENT = "DoTest";

        private DBContextManager cManager = new DBContextManager();

        public IntentController()
        {
            this.srvs = new List<IntentSrv>();
        }

        private void initContext(ref PSAContext context, LUInfo luinfo, string utterance)
        {
            string intent = luinfo.Intent.intent;           

            string coursename = DatetimeUtils.GetCourseName(luinfo);
            TimeRange range = DatetimeUtils.GetTimeRange(luinfo);

            context.Intent = intent;
            context.CourseName = coursename;
            context.timeRange = range;

            context.validTime = DateTime.Now;
        }

        private void updateContext(ref PSAContext context, LUInfo luinfo)
        {
            string intent = luinfo.Intent.intent;

            if (!string.IsNullOrWhiteSpace(intent) && ValidIntent.Contains(intent))
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
        }

        private void SetTestModeContext(ref PSAContext context)
        {
            context.Intent = DOTESTINTENT;                   
        }

        private bool IsTestIntent(string utterance)
        {
            if (!utterance.Contains(ONEONEPREFIX))
                return false;

            string rule = "^(我要做题|我要做数学题|我要做英语题)$";
            string newUtterance = utterance.Replace(ONEONEPREFIX, "").Trim();

            Regex regex = new Regex(rule);
            Match match = regex.Match(newUtterance);

            if (match.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
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

        private TaskFlowContext GetValidTaskFlowContext(string userId)
        {
            ContextInfo ci = cManager.GetContext(userId);

            if (ci == null)
            {
                return null;
            }

            DateTime currentTime = DateTime.Now;
            if (currentTime.Subtract(ci.lastUpdatedTime).TotalMinutes > 30)
            {
                return null;
            }
            else
            {
                TaskFlowContext tfContext = JsonConvert.DeserializeObject<TaskFlowContext>(ci.jsonString);
                return tfContext;
            }
        }
        private PSAContext GetValidPSAContext(string userId)
        {
            ContextInfo ci = cManager.GetContext(userId);
            if (ci == null)
            {
                return null;
            }

            DateTime currentTime = DateTime.Now;
            if (currentTime.Subtract(ci.lastUpdatedTime).TotalMinutes > 30)
            {
                return null;
            }
            else
            {
                PSAContext psaContext = JsonConvert.DeserializeObject<PSAContext>(ci.jsonString);
                return psaContext;
            }
        }

        public string Answer(string userId, string utterance)
        {
            string answer = "";

            string outofScopeStr = "真抱歉，北宝还只是一个宝宝，懂的东西太少，您的问题我没法回答。\r\n不过我会记录下来，尽快解决。谢谢！";
            string GreetingStr = "你好，我叫北宝，是个会说话的小机器人。\r\n我可以告诉你咱们班的课程和兴趣小组安排，还可以帮您记着今天老师布置的任务。\r\n有什么问题就请问吧[微笑]";

            if (string.IsNullOrWhiteSpace(utterance))
            {
                return GreetingStr;
            }
           
            PSAContext psaContext = this.GetValidPSAContext(userId);
            if (psaContext == null)
            {
                psaContext = new PSAContext(userId);                
                cManager.CreateContext(psaContext, userId);
            }
                        
            if (IsTestIntent(utterance))
            {
                TaskFlowContext tfContext = new TaskFlowContext(userId);

                string courseName = null;

                string rule = "英语|数学";
                Regex regex = new Regex(rule);
                Match match = regex.Match(utterance);
                
                if (match.Success)
                {                    
                    courseName = match.Value;
                    tfContext.CourseInTest = courseName;
                }

                this.SetTestModeContext(ref psaContext);
                
                answer = this.taskSrv.StartTest(ref tfContext);
                cManager.CreateContext(tfContext, userId);

            }
            else
            {
                utterance = utterance.Replace(ONEONEPREFIX, "");

                TaskFlowContext tfContext = this.GetValidTaskFlowContext(userId);

                if (tfContext == null || !tfContext.IsInTesting)
                { 
                    LUInfo luinfo = this.luController.Understand(utterance);

                    if (!IsValid(psaContext))
                    {
                        initContext(ref psaContext, luinfo, utterance);
                    }
                    else
                    {
                        updateContext(ref psaContext, luinfo);
                    }

                    //default time is now
                    if(psaContext.timeRange == null)
                    {
                        psaContext.timeRange = new TimeRange();
                        DateTime now = DateTime.Now;
                        psaContext.timeRange.startDate = now;
                        psaContext.timeRange.endDate = now;
                    }
                }                            
            }
            
            switch (psaContext.Intent)
            {
                case "DoTest":
                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        TaskFlowContext tfContext = this.GetValidTaskFlowContext(userId);
                        if (tfContext.CourseInTest == null)
                        {
                            answer = this.taskSrv.GetCourseInTest(ref tfContext, utterance);
                            if (answer != null)
                            { 
                                cManager.UpdateContext(tfContext, userId);
                            }
                        }
                        else
                        {                                               
                            answer = this.taskSrv.ReceiveUserAnswer(ref tfContext, utterance);
                        
                            if (!tfContext.IsInTesting)
                            {
                                cManager.RemoveContext(userId);
                            }
                            else
                            {
                                cManager.UpdateContext(tfContext, userId);
                            }                    
                        }
                    }
                    break;
                case "AskCourseSchedule":
                case "AskDelivery":
                case "AskTrainingSchedule":
                    this.srvs.Add(new ScheduleQuerySrv());
                    break;
                case "AskHomework":
                case "AskNotification":
                    this.srvs.Add(new NotificationQuerySrv());
                    break;
                case "AskStoryContribution":
                    answer = "只要愿意，每一个家长都可以来给孩子们讲故事。\r\n您如果有这个意愿，请在班级群里联系王沐宁妈妈。";
                    break;
                case "AskStorySchedule":
                    answer = "一般情况下是每周四早上上课前有家长到班里讲故事。\r\n根据学校的具体安排，可能时间会变，请您注意老师的通知。";
                    break;                
                case "Greeting": //"Greeting", "None"
                    answer = GreetingStr;//"你好，我叫北宝，是个会说话的小机器人。我可以告诉你咱们班的课程和兴趣小组安排，还可以帮您记着今天老师布置的任务。有什么问题就请问吧[微笑]";//DatetimeUtils.GetOutofScopeAnswer(BotType.PSA);
                    break;
                case "Praise":
                    answer = "谢谢您的夸奖，我会继续努力的。";
                    break;
                case "AskSwimmingClass":
                    answer = "游泳课从5月15日至6月8日，持续4周，第一、三、四周，周一至四体育课时间游泳。\r\n第二周游泳时间，请注意通知。";
                    break;
                default: //case "None":
                    answer = outofScopeStr;//"真抱歉，北宝还只是一个宝宝，懂的东西太少，您的问题我没法回答。不过我会记录下来，尽快解决。谢谢！";
                    break;
            }

            if (string.IsNullOrWhiteSpace(answer))
            { 
                foreach (IntentSrv srv in this.srvs)
                {
                    string singleAnswer = srv.GetAnswer(psaContext);
                    answer += singleAnswer;
                }

                this.srvs.Clear();
            }

            cManager.UpdateContext(psaContext, userId);
                    
            return answer;
        }

        private bool ContainsLocationDateOnly(LUInfo luInfo, string utterance)
        {
            if (luInfo.EntityList == null || luInfo.EntityList.Count == 0)
            {
                return false;
            }

            bool containLocation = false;
            bool containDate = false;
            bool containOther = false;

            string entityStr = "";

            foreach (Entity entity in luInfo.EntityList)
            {
                if (entity.type == "Location")
                {
                    containLocation = true;
                    entityStr += entity.value;
                }
                else if (entity.type == "builtin.datetime.date" || entity.type == "builtin.datetime.time")
                {
                    containDate = true;
                    entityStr += entity.value.Replace(" ", "");
                }
                else
                {
                    containOther = true;
                }
            }


            if (containLocation && containDate && !containOther)
            {
                if (entityStr.Trim().Length == utterance.Trim().Length)
                {
                    return true;
                }
                else if ((((double)entityStr.Trim().Length / (double)utterance.Length) > 0.8) && (luInfo.Intent.intent == "None"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

       /* private bool IsLocationOnly(LUInfo luinfo, string utterance)
        {
            string location = DatetimeUtils.GetLocation(luinfo, false);

            if (string.IsNullOrWhiteSpace(location))
            {
                return false;
            }

            if (location == utterance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }*/
    }
}
