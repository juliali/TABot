using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler.Data
{
    public enum ColumnType
    {
        DateTime, Int, Float, String, LongString, Binary
    }

    public class ColumnSchema
    {
        public int rowIndex { get; set; }

        public ColumnType type { get; set; }

        public string name { get; set; }
    }
}
