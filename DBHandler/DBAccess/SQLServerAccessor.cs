using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler.DBAccess
{
    public class SQLServerAccessor
    {
        private const string connectionString = "Server=tcp:juliatest.database.windows.net,1433;Initial Catalog=PSADB;Persist Security Info=False;User ID=JuliaLi;Password=Azure20090303;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public const string VARBINARYSTR = "@VarBinary";
        private SqlConnection conn;

        public SQLServerAccessor()
        {
            conn = new SqlConnection(connectionString);
            
        }

        public void ExecSQL(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return;

            try
            {
                conn.Open();

                SqlCommand command = new SqlCommand(sql, conn);                
                
                command.ExecuteNonQuery();                                    
                command.Dispose();                
            }
            catch (Exception ex)
            {
                throw new Exception("Can not open connection !", ex);
            }
            finally
            {
                conn.Close();
            }
        }

        public List<Dictionary<string,string>> TableSearch(string[] columns, string sql)
        {
            if (columns == null || columns.Length < 1)
                return null;

            if (string.IsNullOrWhiteSpace(sql))
                return null;

            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            try
            {
                conn.Open();

                SqlCommand command = new SqlCommand(sql, conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {                        
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        foreach(string column in columns)
                        {
                            dictionary.Add(column, reader[column].ToString());
                        }

                        result.Add(dictionary);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Can not open connection !", ex);
            }
            finally
            {
                conn.Close();
            }

            return result;
        }

        public void SaveObject(/*string tableName, string[] otherColumns, string[] otherColumnValues, string objColumnName,*/string sql,  Object obj)
        {
            MemoryStream memStream = new MemoryStream();
            StreamWriter sw = new StreamWriter(memStream);

            sw.Write(obj);

            SqlCommand sqlCmd = new SqlCommand(/*"INSERT INTO dbo." + tableName + "(" + objColumnName+ ") VALUES (" + VARBINARYSTR + ")"*/ sql, conn);

            sqlCmd.Parameters.Add(VARBINARYSTR, SqlDbType.VarBinary, Int32.MaxValue);

            sqlCmd.Parameters[VARBINARYSTR].Value = memStream.GetBuffer();

            sqlCmd.ExecuteNonQuery();
        }
    }
}
