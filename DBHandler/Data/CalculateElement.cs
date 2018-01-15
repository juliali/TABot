using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBHandler.Data
{
    public enum CalculateType
    {
        Operator, Operand
    }

    public enum OperatorItem
    {
        StackTop = 0,
        Plus = 1,
        Minus = 2,
        Times = 3,
        Divides = 4,        
        LeftBrace = 5,
        RightBrace = 6
    }    

    public abstract class CalculateElement
    {
        
        public CalculateType type { get; set; }
    }

    public class Operator:CalculateElement
    {
        public new CalculateType type = CalculateType.Operator;
        private OperatorItem item;

        public static string[] operatorRegexes = { "(=|＝)", "(\\+|＋)", "(-|—|－)", "(x|X|×)", "(/|÷)", "(\\(|（)", "(\\)|）)" };

        //public static string[] operatorRegexes = { "^(=|=)$", "^(+|+)$", "^(-|—|－)$", "^(x|X|×)$", "^(/|÷)$", "^(\\(|（)$", "^(\\)|）)$" };
        public OperatorItem GetOperator()
        {
            return item;
        }

        public Operator()
        {
            this.item = OperatorItem.StackTop;
        }

        public Operator(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new Exception("It is not a Operator");
            }

            string operatorStr = str.Trim();

            //string[] regex = { "^(=|=)$", "^(+|+)$", "^(-|—|－)$", "^(x|X|×)$", "^(/|÷)$", "^(\\(|（)$", "^(\\)|）)$" };

            int index = -1;
            for (int i = 0; i < operatorRegexes.Length; i++)
            {
                string rule = "^" + operatorRegexes[i] + "$";
                Regex r = new Regex(rule, RegexOptions.IgnoreCase);

                // Match the regular expression pattern against a text string.
                Match m = r.Match(str);

                if (m.Success)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                throw new Exception(str + " is not a Operator");
            }

            this.item = (OperatorItem)(index);
        }

        // 0 -- the same priority
        // -1 -- item < other
        // 1 -- item > other
        // 2 -- error
        public int compare(Operator otherOps)
        {
            OperatorItem other = otherOps.GetOperator();

            switch(item)
            {
                case OperatorItem.Plus:
                    switch(other)
                    {
                        case OperatorItem.Plus:                            
                        case OperatorItem.Minus:
                            return 1;
                        case OperatorItem.Times:                            
                        case OperatorItem.Divides:                            
                        case OperatorItem.LeftBrace:
                            return -1;
                        case OperatorItem.RightBrace:                            
                        case OperatorItem.StackTop:
                            return 1;
                    }
                    break;
                case OperatorItem.Minus:
                    switch (other)
                    {
                        case OperatorItem.Plus:
                        case OperatorItem.Minus:
                            return 1;
                        case OperatorItem.Times:
                        case OperatorItem.Divides:
                        case OperatorItem.LeftBrace:
                            return -1;
                        case OperatorItem.RightBrace:
                        case OperatorItem.StackTop:
                            return 1;
                    }
                    break;
                case OperatorItem.Times:
                    switch (other)
                    {
                        case OperatorItem.Plus:                            
                        case OperatorItem.Minus:                            
                        case OperatorItem.Times:                            
                        case OperatorItem.Divides:
                            return 1;
                        case OperatorItem.LeftBrace:
                            return -1;
                        case OperatorItem.RightBrace:                            
                        case OperatorItem.StackTop:
                            return 1;
                    }
                    break;
                case OperatorItem.Divides:
                    switch (other)
                    {
                        case OperatorItem.Plus:
                        case OperatorItem.Minus:
                        case OperatorItem.Times:
                        case OperatorItem.Divides:
                            return 1;
                        case OperatorItem.LeftBrace:
                            return -1;
                        case OperatorItem.RightBrace:
                        case OperatorItem.StackTop:
                            return 1;
                    }
                    break;
                case OperatorItem.LeftBrace:
                    switch (other)
                    {
                        case OperatorItem.Plus:                            
                        case OperatorItem.Minus:                            
                        case OperatorItem.Times:                            
                        case OperatorItem.Divides:                            
                        case OperatorItem.LeftBrace:
                            return -1;
                        case OperatorItem.RightBrace:
                            return 0;
                        case OperatorItem.StackTop:
                            return 2;
                    }
                    break;
                case OperatorItem.RightBrace:
                    switch (other)
                    {
                        case OperatorItem.Plus:                            
                        case OperatorItem.Minus:                            
                        case OperatorItem.Times:                            
                        case OperatorItem.Divides:
                            return 1;
                        case OperatorItem.LeftBrace:
                            return 2;
                        case OperatorItem.RightBrace:                            
                        case OperatorItem.StackTop:
                            return 1;
                    }
                    break;
                case OperatorItem.StackTop:
                    switch (other)
                    {
                        case OperatorItem.Plus:                            
                        case OperatorItem.Minus:                            
                        case OperatorItem.Times:                            
                        case OperatorItem.Divides:                            
                        case OperatorItem.LeftBrace:
                            return -1;                            
                        case OperatorItem.RightBrace:
                            return 2;
                        case OperatorItem.StackTop:
                            return 0;
                    }
                    break;
            }

            return 2;
        }
    }

    public class Operand : CalculateElement
    {
        public new CalculateType type = CalculateType.Operator;
        private int number;

        public Operand(int num)
        {
            this.number = num;
        }

        public Operand(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new Exception("It is not a Operand.");
            }

            string operandStr = str.Trim();

            try
            { 
                this.number = int.Parse(str);
            }
            catch(Exception e)
            {
                throw new Exception(str + " is not a Operand.", e);
            }            
        }

        public int GetNumber()
        {
            return this.number;
        }
    }
}
