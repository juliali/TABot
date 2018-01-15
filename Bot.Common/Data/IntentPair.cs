using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Common.Data
{
    public class IntentPair
    {
        public string text { get; set; }
        public string intentName { get; set; }

        public string[] entityLabels = new string[0];
    }
}
