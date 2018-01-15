using Bot.Common.Data;
using Newtonsoft.Json;
using TA.Engine.Data;
using TA.Engine.TAChecker;
using TA.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TA.Engine.Services
{
       
    public class DICSExamSrv
    {
        private const int TESTITEMCOUNT = 3;
        
        private static TADBChecker checker = new TADBChecker();

        private const string KEYWORD = "性格测试";

        //private Dictionary<int, Dictionary<int, char>> optionConvertor = new Dictionary<int, Dictionary<int, char>>();
        public DICSExamSrv()
        {
        
        }

        //public bool IsTesting(ref ExamSuitContext context)
        //{
         //   return context.IsInTesting;
        //}
        
        private void GenerateExamInfo(ref ExamSuitContext context)
        {
            context.FeatureOptionConvertor = new Dictionary<int, Dictionary<int, char>>();
            context.taskItems = new List<TestItem>();

            string resFileContent = Utility.ReadEmbeddedResourceFile("PSAssitant.Engine.Res.dics.json");
            DISCInfo info = JsonConvert.DeserializeObject<DISCInfo>(resFileContent);

            context.headline = info.HeadLine;


            string commonQuestion = info.Questions.CommonQuestion;
            string commonRegex = info.Questions.CommonAnswerRegex;            

            foreach(DiscQuestionInfo aQues in info.Questions.QuestionOptions)
            {
                int seqNo = aQues.QuestionSeqNo;

                string questionStr = "第" + seqNo.ToString() + "题，" + commonQuestion + "\r\n";
                Dictionary<int, char> featureDict = new Dictionary<int, char>();
                foreach (OptionInfo opt in aQues.options)
                {
                    questionStr += opt.OptionSeqNo + "、" + opt.OptionContent + "\r\n";
                    featureDict.Add(opt.OptionSeqNo, opt.OptionFeature);
                }

                context.FeatureOptionConvertor.Add(seqNo, featureDict);
                TestItem item = new TestItem();
                item.index = seqNo - 1;
                item.question = questionStr;
                item.answerMatchRule = commonRegex;

                context.taskItems.Add(item);
            }

            context.ReviewDict = new Dictionary<char, ReviewInfo>();
            foreach(ReviewInfo rInfo in info.Reviews)
            {
                context.ReviewDict.Add(rInfo.FeatureChar, rInfo);
            }
        }

        private void Init(ref ExamSuitContext context)
        {
            GenerateExamInfo(ref context);
            context.currentIndex = 0;            
        }

        private string GetNextText(ref ExamSuitContext context)
        {
            if (context.status == ESStatus.OnGoing)
            {                
                return GetQuestion(ref context);                
            }
            else
            {                                               
                string result = "";

                if (context.status == ESStatus.Finished)
                {
                    result += GenerateResult(ref context);
                }
                else if (context.status == ESStatus.Paused)
                {
                    result += "中途退出将无法获得测试结果,你之前的测试结果将被保留8小时,下次输入“" + KEYWORD + "”可继续进行测试。\r\n8小时后";
                }

                result += "再输入“" + KEYWORD + "”将重新开始测试";
                return result;
            }
        }

        private string GetQuestion(ref ExamSuitContext context)
        {
            string formula = context.taskItems.ElementAt(context.currentIndex).question;
            return formula;
        }

        private string GetReviewContent(ReviewInfo review)
        {
            string result = "";            
            result += review.Name + "\r\n";

            result += review.ShortDesc + "\r\n";
            result += review.LongDesc + "\r\n";

            result += "标签是：" + review.Labels + "\r\n";

            return result;
        }

        private string GenerateResult(ref ExamSuitContext context)
        {           
            Dictionary<char, int> answerDict = new Dictionary<char, int>();

            foreach (TestItem item in context.taskItems)
            {
                int seqNo = item.index + 1;

                int userAnswer = int.Parse(item.userAnswer);

                char userInputFeature = context.FeatureOptionConvertor[seqNo][userAnswer];

                if (answerDict.Keys.Contains(userInputFeature))
                {
                    answerDict[userInputFeature] ++;
                }
                else
                {
                    answerDict.Add(userInputFeature, 1);
                }                
            }

            List<char> dominatedFeatures = new List<char>();

            int averageNumber = context.taskItems.Count / answerDict.Count;

            string result = "你的选择结果是：";
            foreach (char feature in answerDict.Keys)
            {
                if (answerDict[feature] > averageNumber)
                {
                    dominatedFeatures.Add(feature);
                }

                result += feature + " " + answerDict[feature] + ", ";
            }

            result += "\r\n";

            if (dominatedFeatures.Count == 1)
            {
                result += "你是一个典型的" + GetReviewContent(context.ReviewDict[dominatedFeatures[0]]);                
            }
            else
            {
                result += "你在" + dominatedFeatures.Count.ToString() + "个选项上都有超过" + averageNumber + "个的选项，所以你具备下列两项特征:\r\n";
                foreach(char feature in dominatedFeatures)
                {
                    result += GetReviewContent(context.ReviewDict[feature]);
                }
            }
            
            context.status = ESStatus.Finished;
            return result;
        }

        private void SetTestResult(ref ExamSuitContext context, string result)
        {
            context.taskItems.ElementAt(context.currentIndex++).userAnswer = result;            
        }

        private string AskBack(ref ExamSuitContext context)
        {
            context.AskBackTime++;

            if (context.AskBackTime > ConfigurationData.ASKBACKLIMITTIME)
            {                
                context.status = ESStatus.Paused;

                string result = "";                    
                if (context.currentIndex > 0)
                {
                    result += "中途退出将无法获得测试结果,你之前的测试结果将被保留8小时,下次输入“" + KEYWORD + "”可继续进行测试。\r\n8小时后";
                }
                else //context.currentIndex == 0
                {
                    context.status = ESStatus.Aborded;
                    result += "你已经放弃本次测试。\r\n";
                }

                result +=  "再输入“" + KEYWORD + "”将重新开始测试";
                return result;
                
            }
                         
            return "请输入对应选项序号，不要有其他字符\r\n" + GetNextText(ref context) + "如想退出测试，请输入任何字母或汉字";                        
        }

        public string StartTest(ref ExamSuitContext context, ExamSuitContext cachedContext)
        {                   
            string firstText = "";
            
            if (cachedContext != null)
            {
                firstText += context.ynItem.question;
                context = cachedContext;
                context.status = ESStatus.Restarted;    
            }
            else
            { 
                Init(ref context);
                context.status = ESStatus.OnGoing;
                firstText += "下面我们就要开始答题了，本次测试一共40题，每一道题目显示出来以后，请直接输入你所选择选项的序号。\r\n";
                firstText += this.GetNextText(ref context);
                
            }

            return firstText;
        }

        public string ContinueOrRefresh(ref ExamSuitContext context, string userAnswer)
        {
            bool askBack = false;
            if (string.IsNullOrWhiteSpace(userAnswer))
            {
                askBack = true;
            }
            else
            {                
                string answer = userAnswer.Trim().ToLower();
                string matchedRule = context.ynItem.answerMatchRule;

                Regex regex = new Regex(matchedRule);
                Match match = regex.Match(answer);

                if (match.Success)
                {                    
                    string matchedValue = match.Value;
                    bool IsUseStore = context.ynItem.answerMap[matchedValue.ToLower()];

                    string result = "";
                    if (!IsUseStore)
                    {
                        result += "现在重新开始一轮测试\r\n";
                        Init(ref context);
                    }
                    else
                    {
                        result += "现在测试继续\r\n";
                    }

                    context.status = ESStatus.OnGoing;
                    context.AskBackTime = 0;

                    result += this.GetNextText(ref context);
                   

                    return result;
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

        public string ReceiveUserAnswer(ref ExamSuitContext context, string userAnswer)
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
                        context.taskItems.ElementAt(context.currentIndex++).userAnswer = answer;

                    if (context.currentIndex == context.taskItems.Count)
                    {
                        context.status = ESStatus.Finished;
                    }
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
