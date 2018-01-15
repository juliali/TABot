using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler.Calculate
{
    public class DiscQuestionInfo
    {
        public int QuestionSeqNo;
        public List<OptionInfo> options = new List<OptionInfo>();
    }

    public class OptionInfo
    {
        public int OptionSeqNo;
        public string OptionContent;
        public char OptionFeature;
    }
    public class DiscParser
    {
        public List<DiscQuestionInfo> ParseDiscQuestionFile(string fileName)
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);
            int questionIndex = 0;

            List<DiscQuestionInfo> results = new List<DiscQuestionInfo>();

            DiscQuestionInfo aQuestion = new DiscQuestionInfo();
            aQuestion.QuestionSeqNo = questionIndex + 1;
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    results.Add(aQuestion);
                    questionIndex++;
                    aQuestion = new DiscQuestionInfo();
                    aQuestion.QuestionSeqNo = questionIndex + 1;
                }
                else
                {
                    OptionInfo option = new OptionInfo();
                    string[] tmps = line.Split(';');

                    int optionSeqNo = int.Parse(tmps[0]);
                    string content = tmps[1];
                    char feature = char.Parse(tmps[2]);

                    option.OptionSeqNo = optionSeqNo;
                    option.OptionContent = content;
                    option.OptionFeature = feature;

                    aQuestion.options.Add(option);
                }
            }

            results.Add(aQuestion);

            return results;
        }
    }
}
