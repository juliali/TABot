using DBHandler.Calculate;
using DBHandler.Data;
using DBHandler.DataFileParser;
using DBHandler.DBAccess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBHandler
{
    class Program
    {
        public static void GenerateCreateScript()
        {
            SQLServerAccessor accessor = new SQLServerAccessor();

            string tsvfilepath = "C:\\WS\\OfficialAccountBotExtension\\Hackthon\\TABot\\DBSourceData\\Calculator3.tsv";

            string csvfilepath = "C:\\WS\\OfficialAccountBotExtension\\Hackthon\\TABot\\DBSourceData\\PSAScheduleData2.csv";
            // "C:\\WS\\OfficialAccountBotExtension\\Hackthon\\TABot\\DBSourceData\\PSANotificationData.csv";
            // "C:\\WS\\OfficialAccountBotExtension\\Hackthon\\TABot\\DBSourceData\\PSAESCachedObject.csv";

            CSVParser parser = new CSVParser(csvfilepath);
            //TSVParser parser = new TSVParser(tsvfilepath);
            DBTableSchema schema = parser.ParserSchema();

            string createtablestr = parser.GenerateSQLQueryForCreateTable(schema);
            Console.WriteLine(createtablestr);

            accessor.ExecSQL(createtablestr);

            List<string> insertList = parser.GenerateSQLQueryForInsertRow(schema);
            StringBuilder builder = new StringBuilder();
            foreach(string line in insertList)
            {
                builder.AppendLine(line);
                Console.WriteLine(line);
            }

            accessor.ExecSQL(builder.ToString());
        }

        public static void TextFormulaCalculate()
        {
            List<string> newList = new List<string>();

            string[] lines = //{ "", "63÷(36－29)= " };
                                System.IO.File.ReadAllLines("c:\\users\\build\\Documents\\CALCULATE.txt");

            newList.Add(lines[0]);
            for(int i = 1; i < lines.Length; i ++)
            {
                string line = lines[i];
                string[] tmps = line.Split(',');

                string tmpStr = tmps[0];

                string rule = "\\(\\s*\\)";
                Regex r = new Regex(rule);

                Match m = r.Match(tmpStr);
                
                if (m.Success)
                {
                    Console.WriteLine("*** " + line);
                    newList.Add(line);
                }
                else
                {
                    int result = calculate(tmps[0]);
                    newList.Add(tmps[0] + "," + result.ToString() + "," + tmps[2]);
                }                                    
            }

            System.IO.File.WriteAllLines("c:\\users\\build\\Documents\\Calculator.csv", newList);

            /* string text = "2+3 = ";
             ParseTextFormula parser = new ParseTextFormula();
             List<CalculateElement> elements = parser.Parse(text);

             Calculator calculator = new Calculator();
             int result = calculator.Calculate(elements);
             Console.WriteLine(text + result.ToString());*/
        }

        private static int calculate(string text)
        {
            ParseTextFormula parser = new ParseTextFormula();
            List<CalculateElement> elements = parser.Parse(text);

            Calculator calculator = new Calculator();
            int result = calculator.Calculate(elements);
            Console.WriteLine(text + result.ToString());
            return result;
        }

        public static void ParseDisc()
        {
            DiscParser parser = new DiscParser();
            List<DiscQuestionInfo> result = parser.ParseDiscQuestionFile("D:\\data\\psa\\dics_questions.txt");
            string json = JsonConvert.SerializeObject(result);
            System.IO.File.WriteAllText("D:\\data\\psa\\dics_questions.json", json);
        }

        public static void Main(string[] args)
        {
            GenerateCreateScript();
            //TextFormulaCalculate();
           // ParseDisc();
            Console.WriteLine("Finished!");
        }
    }
}
