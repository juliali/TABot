using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TA.Engine.Data
{
    public class DISCInfo
    {
        public string HeadLine;
        public QuestionSet Questions;
        public List<ReviewInfo> Reviews;
    }

    public class QuestionSet
    {
        public string CommonQuestion;
        public string CommonAnswerRegex;
        public List<DiscQuestionInfo> QuestionOptions;
    }

    public class ReviewInfo
    {
        public char FeatureChar;
        public string Name;
        public string ShortDesc;
        public string LongDesc;
        public string Labels;
    }

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
}
