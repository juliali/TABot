using DBHandler.Data;
using TA.Engine.TAChecker;
using System;
using System.Collections.Generic;
using TA.Engine.Controllers;

namespace WechatBotFramework.Controllers
{
    public class NotifyResponse
    {
        public bool success = false;
        public string msg;
    }

    public class TAController
    {
        private DialogController bot = new DialogController();
        private const string notificationTableName = "psanotificationdata";
        private TADBChecker dbChecker = new TADBChecker();

        public string AnswerOneOneQuestion(string userId, string question)
        {
            string newQuestion = bot.ONEONEPREFIX + question;
            return AnswerQuestion(userId, newQuestion);
        }
        public string AnswerQuestion(string userId, string question)
        {
            string answer = bot.Answer(userId, question);
            return answer;
        }

        public NotifyResponse NotificationStore(string notifier, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                content = content.Replace("'", "''");
            }

            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyyy-MM-dd");

            string[] columns = { "Timestamp", "DateTime", "Notifier", "Content" };
            ColumnType[] types = { ColumnType.DateTime, ColumnType.String, ColumnType.String, ColumnType.String };

            List<string[]> values = new List<string[]>();
            string[] aValue = new string[columns.Length];
            aValue[0] = now.ToString("yyyy-MM-dd HH:mm:ss");
            aValue[1] = dateStr;
            aValue[2] = notifier;
            aValue[3] = content;

            values.Add(aValue);

            dbChecker.InserTable(notificationTableName, columns, types, values);

            NotifyResponse resp = new NotifyResponse();
            resp.msg = "Notification is sotred successfully.";
            resp.success = true;
            
            return resp;
        }
    }
}
