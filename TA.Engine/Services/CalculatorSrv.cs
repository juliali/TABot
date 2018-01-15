using Bot.Common.Data;
using TA.Engine.Data;
using TA.Engine.TAChecker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TA.Engine.Services
{
       
    public class CalculatorSrv
    {
        private const int TESTITEMCOUNT = 3;
        
        private static TADBChecker checker = new TADBChecker();

        public CalculatorSrv()
        {
        
        }

        public bool IsTesting(ref TaskFlowContext context)
        {
            return context.IsInTesting;
        }
        
        private void Init(ref TaskFlowContext context)
        {            
            context.taskItems = checker.GenerateTestItems(ConfigurationData.TASKITEMCOUNT, context.CourseInTest);
            context.currentIndex = 0;
            context.IsInTesting = true;            
        }

        private string GetNextText(ref TaskFlowContext context)
        {
            if (context.IsInTesting)
            {                
                return GetFormula(ref context);                
            }
            else
            {
                if (context.CourseInTest != null)
                { 
                    return GenerateResult(ref context);
                }
                else
                {
                    context.IsInTesting = false;
                    return "测试中止\r\n想重新开始测试请输入“我要做题”。";
                }
            }
        }

        private string GetFormula(ref TaskFlowContext context)
        {
            string formula = context.taskItems.ElementAt(context.currentIndex).question;
            return formula;
        }

        private string GenerateResult(ref TaskFlowContext context)
        {
            context.IsInTesting = false;

            int correctNum = 0;
            int testedNum = 0;
            int TotalTestItemCount = 0;

            List<string> errorItems = new List<string>();

            foreach (TestItem item in context.taskItems)
            {
                  if (item.userAnswer != null && item.correctResult != null && item.userAnswer.Trim().ToLower() == item.correctResult.Trim().ToLower())
                    {
                        correctNum++;
                        testedNum++;
                    }
                    else if (item.userAnswer != null)
                    {
                        string errorLine = item.question + " " + item.userAnswer + " (X) " + "[" + item.correctResult + "]";
                        errorItems.Add(errorLine);
                        testedNum++;
                    }
                    TotalTestItemCount++;
                
            }

            double percentage = (double)correctNum / (double)testedNum;


            string text = "";            

            if (testedNum == TotalTestItemCount)
            {
                int score = (int)(percentage * 100);
                text += "你得了" + score.ToString() + "分。";
                if (score >= 80)
                {
                    text += "真是太棒了！继续努力哦[微笑]\r\n";
                }
                else if (score >= 60)
                {
                    text += "还是不错的，不过最好加强训练了。[微笑]\r\n";
                }
                else
                {
                    text += "宝宝要主动练习哦！[微笑]\r\n";
                }
            }

            text += "你一共完成" + testedNum.ToString() + "道题，" + correctNum.ToString() + "道正确。";
            if (errorItems.Count > 0)
            {
                text += "\r\n做错的题如下：\r\n";
                text += string.Join("\r\n", errorItems);
            }
            return text;
        }

        private void SetTestResult(ref TaskFlowContext context, string result)
        {
            context.taskItems.ElementAt(context.currentIndex++).userAnswer = result;
            if (context.currentIndex == context.taskItems.Count)
            {
                context.IsInTesting = false;
            }
        }

        private string AskBack(ref TaskFlowContext context)
        {
            context.AskBackTime++;

            if (context.AskBackTime > ConfigurationData.ASKBACKLIMITTIME)
            {
                context.IsInTesting = false;

                if (context.CourseInTest != null)
                { 
                    return "测试中止\r\n" + GenerateResult(ref context);
                }
                else
                {
                    return "测试中止\r\n想重新开始测试请输入“我要做题”。";
                }
            }

            if (context.CourseInTest == "数学")
            { 
                return "做题的过程中请只输入答案的数字，不要有其他字符。\r\n如果还要继续做题请重新输入这道题答案，否则请随便输入一个字母。\r\n" + GetNextText(ref context);
            }
            else if (context.CourseInTest == "英语")// 英语
            {
                return "做题的过程中请只输入答案的英语单词，不要有其他字符。\r\n如果还要继续做题请重新输入这道题答案，否则请随便输入一个数字或汉字。\r\n" + GetNextText(ref context);
            }
            else
            {
                return "请仅输入对应科目的字母，不要有其他字符\r\n" + context.courseSelectionItem.question;
            }            
        }

        public string StartTest(ref TaskFlowContext context)
        {                   
            string firstText = "";
            if (context.CourseInTest == "数学")
            {
                Init(ref context);

                firstText += "下面我们就要开始答题了，每一道题目显示出来以后，请直接输入答案。\r\n比如题目是：“2 + 3 = ”,那么直接输入“5”。\r\n准备好了吗？现在开始做题啦。\r\n";
                firstText += this.GetNextText(ref context);
            }
            else if (context.CourseInTest == "英语")
            {
                Init(ref context);

                firstText += "下面我们就要开始答题了，每一个中文单词显示出来以后，请直接输入对应英文单词。\r\n比如题目是：“苹果”,那么直接输入“apple”。\r\n准备好了吗？现在开始做题啦。\r\n";
                firstText += this.GetNextText(ref context);
            }
            else
            {
                context.IsInTesting = true;

                CourseSelectionTaskItem selectionTask = new CourseSelectionTaskItem();
                context.courseSelectionItem = selectionTask;

                firstText += selectionTask.question;
            }
            
            return firstText;
        }
        
        public string GetCourseInTest(ref TaskFlowContext context, string userAnswer)
        {
            bool askBack = false;
            if (string.IsNullOrWhiteSpace(userAnswer))
            {
                askBack = true;
            }
            else
            {
                string answer = userAnswer.Trim().ToLower();
                string matchedRule = context.courseSelectionItem.answerMatchRule;

                Regex regex = new Regex(matchedRule);
                Match match = regex.Match(answer);

                if (match.Success)
                {
                    //this.SetTestResult(ref context, answer);
                    string matchedValue = match.Value;
                    string courseName = context.courseSelectionItem.answerMap[matchedValue];
                    context.CourseInTest = courseName;

                    Init(ref context);

                    return this.GetNextText(ref context);
                }
                else
                {
                    askBack = true;
                }
            }
            if (askBack)
            {
                return this.AskBack(ref context);
            }

            return null;
        }

        public string ReceiveUserAnswer(ref TaskFlowContext context, string userAnswer)
        {
            bool askBack = false;
            if (string.IsNullOrWhiteSpace(userAnswer))
            {
                askBack = true;
            }
            else
            {
                userAnswer = userAnswer.Trim();
                
                    string answer = userAnswer;

                    if (!string.IsNullOrWhiteSpace(answer))
                    {
                        answer = answer.Trim().ToLower();
                    }

                    string matchedRule = context.taskItems[context.currentIndex].answerMatchRule;

                    Regex regex = new Regex(matchedRule);
                    Match match = regex.Match(answer);

                    if (match.Success)
                    { 
                        this.SetTestResult(ref context, answer);

                        return this.GetNextText(ref context);
                    }
                    else
                    {
                        askBack = true;
                    }
                                 
            }

            if (askBack)
            {                
                return this.AskBack(ref context);                
            }

            return null;
        }        
    }
}
