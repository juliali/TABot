using DBHandler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBHandler.Calculate
{
    public class ParseTextFormula
    {
        
        public List<CalculateElement> Parse(string text)
        {
            List<CalculateElement> list = new List<CalculateElement>();

            string rule = string.Join("|", Operator.operatorRegexes);
            
            Regex r = new Regex(rule);//, RegexOptions.IgnoreCase);

            
            Match m = r.Match(text);
            
            int index = 0;
            while (m.Success)
            {
                
                Group g = m.Groups[0];
                
                CaptureCollection cc = g.Captures;
                Capture c = cc[0];
                int operatorStart = c.Index;
                int operatorLen = c.Length;

                int operandStart = index;
                int operandLen = operatorStart - operandStart;

                string operandStr = text.Substring(operandStart, operandLen);
                if (!string.IsNullOrWhiteSpace(operandStr))
                {
                    Operand operand = new Operand(operandStr);
                    list.Add(operand);
                }

                string operatorStr = text.Substring(operatorStart, operatorLen);
                 
                    Operator oper = new Operator(operatorStr);
                    list.Add(oper);

                    if (oper.GetOperator() == OperatorItem.StackTop)
                    {
                        break;
                    }
                               
                index = operatorStart + operatorLen;  
                
                m = m.NextMatch();
            }            

            return list;
        }
    }
}
