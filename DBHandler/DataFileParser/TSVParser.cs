using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler.DataFileParser
{
    public class TSVParser : TableFileParser
    {

        public TSVParser(string filepath) : base(".tsv", '\t', filepath)
        {

        }
    }
}
