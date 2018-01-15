using Bot.Common.Data;
using Bot.Common.LUEngines;
using Bot.Common.Utils;
using Newtonsoft.Json;
using TA.Engine.Controllers.Component;
using TA.Engine.Data;
using TA.Engine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Engines
{
    public class ChatTableEngine
    {
        private List<IntentSrv> srvs = new List<IntentSrv>();                
              
        public string Answer(string userId, ref PSAContext psaContext)
        {
            string outofScopeStr = "真抱歉，北宝还只是一个宝宝，懂的东西太少，您的问题我没法回答。\r\n不过我会记录下来，尽快解决。谢谢！";
            string GreetingStr = "你好，我叫北宝，是个会说话的小机器人。\r\n我可以告诉你咱们班的课程和兴趣小组安排，还可以帮您记着今天老师布置的任务。\r\n有什么问题就请问吧[微笑]\r\n加我为好友好，单独发送“我要做题”给我，可以做小测验，发送“性格测试”可以测试职场性格";

            string answer = "";            
            
            switch (psaContext.Intent)
            {
                case "AskTest":
                    answer = "输入“我要做题”四个字，并选择要测试的科目，即可开始进行一次小测验。北宝出一道题，请您答一道题，最后北宝会给出本次小测验的结果。";
                    break;
                case "AskExam":
                    answer = "DICS测试是1928年美国心理学家威廉.莫尔顿.马斯顿创建的一套性格测试标准。\r\n目前很多大企业和猎头公司招聘人才时会用到。大家不妨了解一下自己行为模式。\r\n输入“性格测试”四个字，开始测试！";
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
                case "Greeting": 
                    answer = GreetingStr;
                    break;
                case "Praise":
                    answer = "谢谢您的夸奖，我会继续努力的。";
                    break;
                case "AskSwimmingClass":
                    answer = "游泳课已经结束，本学期不再有游泳课。";
                    break;
                default: 
                    answer = outofScopeStr;
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
           
            return answer;

        }
    }
}
