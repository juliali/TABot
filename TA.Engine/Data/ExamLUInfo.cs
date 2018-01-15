using Bot.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public class TestLUInfo: LUInfo
    {
        public bool IsStartStep;
        public string UserInput;
    }

    public class ExamLUInfo: LUInfo
    {
        //public bool IsStartStep;        
        public string UserInput;        
    }
}
