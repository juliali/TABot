using DBHandler.Data;
using Newtonsoft.Json;
using TA.Engine.TAChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace TAService.Controllers
{
    public class Notification
    {
        public string notifier { get; set; }
        public string content { get; set; }
    }

    class NotificationJson
    {
        public string msg { get; set; }
        public string status { get; set; }
    }

    public class TANotificationStoreController : ApiController
    {
        private const string tableName = "psanotificationdata";
        private TADBChecker dbChecker = new TADBChecker();

        // POST api/psanotificationstore
        public string Post([FromBody] Notification notification)
        {
            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyyy-MM-dd");

            string[] columns = { "Timestamp", "DateTime", "Notifier", "Content" };
            ColumnType[] types = { ColumnType.DateTime, ColumnType.String, ColumnType.String, ColumnType.String };

            List<string[]> values = new List<string[]>();
            string[] aValue = new string[columns.Length];
            aValue[0] = now.ToString("yyyy-MM-dd HH:mm:ss");
            aValue[1] = dateStr;
            aValue[2] = notification.notifier;
            aValue[3] = notification.content;

            values.Add(aValue);

            dbChecker.InserTable(tableName, columns, types, values);
           
            NotificationJson json = new NotificationJson();
            json.msg = "Notification is sotred successfully.";
            json.status = "ok";

            string respStr = JsonConvert.SerializeObject(json);

            return respStr;
        }
    }
}