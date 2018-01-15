using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public class ContextInfo
    {
        public string jsonString { get; set; }
        public DateTime lastUpdatedTime { get; set; }

        public ContextType type { get; set; }
    }
}
