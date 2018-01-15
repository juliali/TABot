using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public class YesOrNoTaskItem : TaskItem
    {
        public new int index = 0;
        public readonly new string answerMatchRule = "^[ynYN]$";
        public readonly new string question = "是保留上次的已测试部分继续进行,还是重新开始一轮新的测试？\r\n选择继续上次的结果请输入“Y”，重新开始请输入“N”";

        public readonly Dictionary<string, bool> answerMap = new Dictionary<string, bool>() { { "y", true }, { "n", false } };
    }

    public enum ESStatus
    {
        Started, OnGoing, Paused, Restarted, Finished, Aborded
    }
    public class ExamSuitContext : BotContext
    {
        public ExamSuitContext(string userId)
        {
            this.userId = userId;
            this.type = ContextType.ExamSuitContext;
        }

        
        public List<TestItem> taskItems = null;
        public int currentIndex = -1;
        public int AskBackTime = 0;
               
        public string UserInput = null;

        public Dictionary<int, Dictionary<int, char>> FeatureOptionConvertor = null;
        public string headline = null;
        public Dictionary<char, ReviewInfo> ReviewDict = null;
        public YesOrNoTaskItem ynItem = new YesOrNoTaskItem();

        public ESStatus status = ESStatus.Started;
    }
}
