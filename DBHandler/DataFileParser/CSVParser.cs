using DBHandler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler.DataFileParser
{
    public class CSVParser : TableFileParser
    {

        public CSVParser(string filepath) : base(".csv", ',', filepath)
        {
        
        }
    }
}
