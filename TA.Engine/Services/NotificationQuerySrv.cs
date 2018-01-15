using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Common.Data;
using DBHandler.Data;
using TA.Engine.TAChecker;
using Bot.Common.Utils;
using TA.Engine.Data;

namespace TA.Engine.Services
{
    public class NotificationQuerySrv : IntentSrv
    {
        private const string tableName = "psanotificationdata";
        private TADBChecker dbChecker = new TADBChecker();

        public override string GetAnswer(PSAContext context/*, LUInfo luInfo*/)
        {
            DateTime now = DateTime.Now;

            if(context.timeRange != null && context.timeRange.startDate != null)
            {
                now = context.timeRange.startDate;
            }
            
            
            List<string> conditions = new List<string>();

            string dateStr = context.timeRange.startDate.ToString("yyyy-MM-dd");
            if (context.timeRange.startDate == context.timeRange.endDate)
            {                 
                conditions.Add("DateTime=N'" + dateStr + "'");
            }
            else
            {
                dateStr = context.timeRange.startDate.ToString("yyyy-MM-dd") + " ~ " + context.timeRange.endDate.ToString("yyyy-MM-dd");

                conditions.Add("Timestamp>='" + context.timeRange.startDate.ToString("yyyy-MM-dd") + "'");
                conditions.Add("Timestamp<'" + context.timeRange.endDate.AddDays(1).ToString("yyyy-MM-dd") + "'");
            }

            string[] columns = { "DateTime", "Notifier", "Content" };

            List<Dictionary<string, string>> results = dbChecker.SearchTable(tableName, columns, conditions);

            StringBuilder builder = new StringBuilder();

            if (results == null || results.Count == 0)
            {
                builder.AppendLine(dateStr + " 没有通知");                
            }
            else
            { 
                foreach (Dictionary<string, string> dict in results)
                {
                    string line = "";
                    line += dict["DateTime"] + " ";

                    if (dict["Notifier"] != "YJL SOEVPM")
                    {
                        line += dict["Notifier"] + "通知: ";
                    }

                    string content = dict["Content"];
                    if (!string.IsNullOrWhiteSpace(content))
                        content = content.Replace("<br/>", "\r\n");

                    line += content;

                    builder.AppendLine(line);
                }
            }

            string queryresult = builder.ToString();

            return queryresult;
        }

        /*
        public void AddNotification(string notifier, string content)
        {
            DateTime now = DateTime.Now;
            string dateStr = now.Date.ToString("yyyy-MM-dd");

            string[] columns = { "Timestamp", "DateTime", "Notifier", "Content" };
            ColumnType[] types = { ColumnType.DateTime, ColumnType.String, ColumnType.String, ColumnType.String };

            List<string[]> values = new List<string[]>();
            string[] aValue = new string[columns.Length];
            aValue[0] = now.ToLongTimeString();
            aValue[1] = dateStr;
            aValue[2] = notifier;
            aValue[3] = content;

            values.Add(aValue);

            dbChecker.InserTable(tableName, columns, types, values);

            return;
        }
        */
    }
}
