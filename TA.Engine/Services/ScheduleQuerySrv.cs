using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Common.Data;
using Bot.Common.Utils;
using TA.Engine.TAChecker;
using TA.Engine.Data;

namespace TA.Engine.Services
{
    public class ScheduleQuerySrv : IntentSrv
    {
        private const string tableName = "psascheduledata2";
        private TADBChecker dbChecker = new TADBChecker();

        public override string GetAnswer(PSAContext context)
        {            
            TimeRange range = context.timeRange;
            
            string type = "Course";

            switch(context.Intent)
            {
                case "AskCourseSchedule":
                    type = "Course";
                    break;
                case "AskDelivery":
                    type = "Delivery";
                    break;
                case "AskTrainingSchedule":
                    type = "Training";
                    break;
            }

            string typeCondition = " Type=N'" + type + "' ";            

            string dateCondition = "Date >= '" + range.startDate.ToString("yyyy-MM-dd") + "' ";

            if (range.endDate != null)
            {
                dateCondition += " AND Date <= '" + range.endDate.ToString("yyyy-MM-dd") + "' ";
            }

            string weekdaystr = DatetimeUtils.GetTodayWeekDayString(range.startDate);

            string weekdayCondition = " Weekday=N'" + weekdaystr + "'";

            string timesectionCondition = null;
            if (context.Intent == "AskCourseSchedule" && range.startDate != null && range.endDate != null && range.endDate != range.startDate)
            {
                if (range.endDate.Subtract(range.startDate).Hours < 24)
                {
                    if (range.startDate.Hour == 6)
                    {
                        timesectionCondition = " TimeSector=N'上午' ";
                    }
                    else if (range.startDate.Hour == 12)
                    {
                        timesectionCondition = " TimeSector=N'下午' ";
                    }
                }
            }

            string[] columns = {"Date", "Weekday", "TimeSector", "StartTime", "EndTime", "Name", "Condition", "IsActive"};

            string activeCondition = " IsActive=1 ";

            List<string> conditionsFirstRound = new List<string>();
            conditionsFirstRound.Add(activeCondition);
            conditionsFirstRound.Add(typeCondition);
            conditionsFirstRound.Add(dateCondition);
            if (!string.IsNullOrWhiteSpace(timesectionCondition))
            {
                conditionsFirstRound.Add(timesectionCondition);
            }

            List<string> conditionsSecondRound = new List<string>();
            conditionsSecondRound.Add(activeCondition);
            conditionsSecondRound.Add(typeCondition);
            conditionsSecondRound.Add(weekdayCondition);
            if (!string.IsNullOrWhiteSpace(timesectionCondition))
            {
                conditionsSecondRound.Add(timesectionCondition);
            }
            
            List<Dictionary<string, string>> results = dbChecker.SearchTable(tableName, columns, conditionsFirstRound);

            if (results == null || results.Count == 0)
            {
                results = dbChecker.SearchTable(tableName, columns, conditionsSecondRound);
            }

            string result = "";
            switch (context.Intent)
            {                
                case "AskCourseSchedule":
                    
                    if (results == null ||  results.Count == 0)
                    {
                        result += weekdaystr + "没有安排课程";
                        break;
                    }

                    result +="课程安排\r\n";

                    foreach (Dictionary<string,string> dict in results)
                    {
                        string line = "";

                        if (!string.IsNullOrWhiteSpace(dict["Date"]))
                        {
                            line += DateTime.Parse(dict["Date"]).ToString("yyyy-MM-dd") + " ";
                        }

                        line += dict["Weekday"] + dict["TimeSector"] + " " + dict["Name"];
                        result += line + "\r\n";
                    }
                    break;
                case "AskDelivery":

                    if (results == null || results.Count == 0)
                    {
                        result += weekdaystr + "不上学，不需要家长接送";
                        break;
                    }

                    foreach (Dictionary<string, string> dict in results)
                    {
                        string line = "";
                        if (!string.IsNullOrWhiteSpace(dict["Date"]))
                        {
                            line += DateTime.Parse(dict["Date"]).ToString("yyyy-MM-dd") + " ";
                        }
                        line += dict["Weekday"] + dict["TimeSector"] + "," + dict["Condition"] + " " +  dict["StartTime"] + " " + dict["Name"];
                        result += line + "\r\n";
                    }
                    break;
                case "AskTrainingSchedule":
                    if (results == null || results.Count == 0)
                    {
                        result += weekdaystr + "没有兴趣小组";
                        break;
                    }

                    result += "兴趣小组\r\n";
                    foreach (Dictionary<string, string> dict in results)
                    {
                        string line = "";
                        if (!string.IsNullOrWhiteSpace(dict["Date"]))
                        {
                            line += DateTime.Parse(dict["Date"]).ToString("yyyy-MM-dd") + " ";
                        }
                        line += dict["Weekday"] + " " +  dict["Name"];
                        result +=line + "\r\n";
                    }
                    break;
            }            
            
            return result;                        
        }
    }
}
