using DBHandler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler.DataFileParser
{
    public abstract class TableFileParser
    {
        protected string suffix;
        protected char separator;

        public string filepath;

        public string tableName;

        public string headerLine;
        public string[] dataLines;

        public TableFileParser(string suffix, char separator, string filepath)
        {
            this.suffix = suffix;
            this.separator = separator;

            this.filepath = filepath;
            this.tableName = GetTableName();

            string[] lines = System.IO.File.ReadAllLines(this.filepath, Encoding.UTF8);

            this.headerLine = lines[0];

            dataLines = new string[lines.Length - 1];

            Array.Copy(lines, 1, dataLines, 0, dataLines.Length);
        }

        private string GetTableName()
        {
            string[] tableNameStrs = filepath.Split('\\');
            string tableName = tableNameStrs[tableNameStrs.Length - 1];

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                tableName = tableName.ToLower().Replace(suffix.ToLower(), string.Empty);
            }

            return tableName;
        }

        public DBTableSchema ParserSchema()
        {
            DBTableSchema schema = new DBTableSchema();

            schema.tableName = tableName;

            string[] headers = headerLine.Split(separator);
            int len = headers.Length;

            if (len < 1)
            {
                return null;
            }

            ColumnSchema[] columns = new ColumnSchema[len];
            for (int i = 0; i < len; i++)
            {
                ColumnSchema column = new ColumnSchema();
                column.rowIndex = i;

                string columnStr = headers[i];
                string[] tmps = columnStr.Split(':');
                
                if (tmps.Length == 1)
                {
                    column.type = ColumnType.String;
                    column.name = tmps[0];
                }
                else if (tmps.Length == 2)
                {
                    column.type = (ColumnType)Enum.Parse(typeof(ColumnType), tmps[1], true);
                    column.name = tmps[0];
                }

                columns[i] = column;
            }

            schema.headers = columns;

            return schema;
        }

        public string GenerateSQLQueryForCreateTable(DBTableSchema schema)
        {
            string tableName = "dbo." + schema.tableName;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DROP TABLE IF EXISTS " + tableName);
            builder.AppendLine("CREATE TABLE " + tableName + " (");
            builder.AppendLine("Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,");
            foreach (ColumnSchema column in schema.headers)
            {
                string line = column.name + " ";
                string type = "nvarchar(256)";

                if (column.type == ColumnType.Int)
                {
                    type = "INT";
                }
                else if (column.type == ColumnType.Float)
                {
                    type = "FLOAT";
                }
                else if (column.type == ColumnType.DateTime)
                {
                    type = "DateTime";
                }
                else if (column.type == ColumnType.LongString)
                {
                    type = "nvarchar(MAX)";
                }
                else if (column.type == ColumnType.Binary)
                {
                    type = "VARBINARY(MAX)";
                }

                line += type + " NULL,";
                builder.AppendLine(line);
            }

            string result = builder.ToString(0, builder.Length - 3);
            result += "\r\n)";

            return result;
        }

        public List<string> GenerateSQLQueryForInsertRow(DBTableSchema schema)
        {
            string tableName = schema.tableName;
            List<string> results = new List<string>();

            foreach (string line in dataLines)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("INSERT INTO dbo." + tableName + "(");
                string[] tmps = line.Split(separator);
                string columns = "";
                string values = "";
                for (int i = 0; i < tmps.Length; i++)
                {

                    if (!string.IsNullOrWhiteSpace(tmps[i]))
                    {
                        columns += schema.headers[i].name + ",";
                        if ((schema.headers[i].type == ColumnType.Int) || (schema.headers[i].type == ColumnType.Float))
                        {
                            values += tmps[i] + ",";
                        }
                        else
                        {
                            values += "N'" + tmps[i] + "',";
                        }
                    }
                }

                columns = columns.Substring(0, columns.Length - 1);
                values = values.Substring(0, values.Length - 1);

                builder.Append(columns + ") VALUES (" + values + ");");

                results.Add(builder.ToString());
            }

            return results;
        }
    }
}

