using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{    
    public abstract class TaskItem
    {
        public int index { get; set; } = -1;
        public string question { get; set; }
        public string userAnswer { get; set; } = null;
        public string answerMatchRule { get; set; }
    }

    public class TestItem : TaskItem
    {
        public string correctResult { get; set; }
    }

    public class CourseSelectionTaskItem: TaskItem
    {        
        //public string userSelectedCourseName { get; set; } = null;

        public new int index = 0;
        public readonly new string answerMatchRule = "^[a-bA-B]$";
        public readonly new string question = "请输入选项字母，来选择你想做练习题的科目：\r\n(a)数学 (b)英语";

        public readonly Dictionary<string, string> answerMap = new Dictionary<string, string>() { {"a", "数学" }, {"b", "英语" } };
    }



}
