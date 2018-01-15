using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public class TaskFlowContext : BotContext
    {
        public TaskFlowContext(string userId)
        {
            this.userId = userId;
            this.type = ContextType.TaskFlowContext;
        }        

        public bool IsInTesting = false;

        public List<TestItem> taskItems = null;
        public int currentIndex = -1;
        public int AskBackTime = 0;
        public string CourseInTest = null;
        public CourseSelectionTaskItem courseSelectionItem = null;

        public string UserInput = null;       
    }
}
