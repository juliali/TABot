using DBHandler.Data;
using Newtonsoft.Json;
using TA.Engine.Data;
using TA.Engine.TAChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Controllers.Component
{
    public class DBContextManager: IContextManager
    {
        private const string tableName = "PSAObject";
        private const string cacheTableName = "PSAESCachedObject";

        private TADBChecker dbChecker = new TADBChecker();
        
        public void StoreESContext(ExamSuitContext esContext, string userId)
        {
            string json = JsonConvert.SerializeObject(esContext);

            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyyy-MM-dd");

            string[] columns = { "UserId", "Timestamp", "JsonObject" };
            ColumnType[] types = { ColumnType.String, ColumnType.DateTime, ColumnType.String };

            List<string[]> values = new List<string[]>();
            string[] aValue = new string[columns.Length];
            aValue[0] = userId;
            aValue[1] = now.ToString("yyyy-MM-dd HH:mm:ss");           
            aValue[2] = json;

            values.Add(aValue);

            dbChecker.InserTable(cacheTableName, columns, types, values);
        }

        public ContextInfo GetAndRemoveESContext(string userId)
        {
            List<string> conditions = new List<string>();
            conditions.Add("UserId = N'" + userId + "'");

            string[] columns = { "Timestamp", "JsonObject" };

            List<Dictionary<string, string>> results = dbChecker.GetAndDeleteRecords(cacheTableName, columns, conditions);

            if (results == null || results.Count == 0)
            {
                return null;
            }
            else
            {
                string json = results[0]["JsonObject"];
                string dateStr = results[0]["Timestamp"];
                ContextType type = ContextType.ExamSuitContext;

                ContextInfo ci = new ContextInfo();
                ci.jsonString = json;
                ci.lastUpdatedTime = DateTime.Parse(dateStr);
                ci.type = type;

                return ci;
            }
        }

        public void CreateContext(BotContext context, string userId)
        {      
            
            RemoveContext(userId);
                  
            string json = JsonConvert.SerializeObject(context);

            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyyy-MM-dd");

            string[] columns = { "UserId", "Timestamp", "ClassType", "JsonObject" };
            ColumnType[] types = { ColumnType.String, ColumnType.DateTime, ColumnType.String, ColumnType.String };

            List<string[]> values = new List<string[]>();
            string[] aValue = new string[columns.Length];
            aValue[0] = userId;
            aValue[1] = now.ToString("yyyy-MM-dd HH:mm:ss");
            aValue[2] = context.type.ToString();         
            aValue[3] = json;

            values.Add(aValue);            

            dbChecker.InserTable(tableName, columns, types, values);
        }

        
        public void UpdateContext(BotContext context, string userId)
        {
            List<string> conditions = new List<string>();
            conditions.Add("UserId = N'" + userId + "'");            

            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyyy-MM-dd");

            string[] columns = { "Timestamp", "JsonObject" };
            ColumnType[] types = { ColumnType.DateTime, ColumnType.String };

            string json = JsonConvert.SerializeObject(context);

            string[] values = { now.ToString("yyyy-MM-dd HH:mm:ss"), json };

            dbChecker.UpdateTable(tableName, columns, types, values, conditions);
        }
        

        public ContextInfo GetContext(string userId)
        {
            List<string> conditions = new List<string>();
            conditions.Add("UserId = N'" + userId + "'");            

            string[] columns = { "Timestamp", "ClassType", "JsonObject" };

            List<Dictionary<string, string>> results = dbChecker.SearchTable(tableName, columns, conditions);

            if (results == null || results.Count == 0)
            {
                return null;
            }
            else
            {
                string json = results[0]["JsonObject"];
                string dateStr = results[0]["Timestamp"];
                ContextType type =(ContextType) Enum.Parse(typeof(ContextType), results[0]["ClassType"]);

                ContextInfo ci = new ContextInfo();
                ci.jsonString = json;
                ci.lastUpdatedTime = DateTime.Parse(dateStr);
                ci.type = type;
                
                return ci;
            }
        }

        public void RemoveContext(string userId)
        {
            List<string> conditions = new List<string>();
            conditions.Add("UserId = N'" + userId + "'");            

            dbChecker.DeleteTable(tableName, conditions);
        }
    }

}
