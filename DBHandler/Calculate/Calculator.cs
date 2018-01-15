using DBHandler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHandler.Calculate
{
    public class Calculator
    {
        private List<CalculateElement> newElementList;
        private Stack<Operator> operatorStack;

        public Calculator()
        {
            this.newElementList = new List<CalculateElement>();
            this.operatorStack = new Stack<Operator>();
            this.operatorStack.Push(new Operator());
        }

        private void Preprocess(CalculateElement element)
        {
            if (element is Operand)
            {
                this.newElementList.Add(element);
            }
            else
            {
                Operator x1 = this.operatorStack.Peek();
                Operator x2 = (Operator)element;

                int compareResult = x1.compare(x2);

                if (compareResult == -1)
                {
                    this.operatorStack.Push(x2);
                }
                else if (compareResult == 0)
                {
                    if (x1.GetOperator() == OperatorItem.LeftBrace && x2.GetOperator() == OperatorItem.RightBrace)
                    {
                        this.operatorStack.Pop();
                    }
                    else if (x1.GetOperator() == OperatorItem.StackTop && x2.GetOperator() == OperatorItem.StackTop)
                    {
                        return;
                    }
                }
                else if (compareResult == 1)
                {
                    //x1 = this.operatorStack.Pop();

                    x1 = this.operatorStack.Pop();
                    this.newElementList.Add(x1);

                    Preprocess(element);
                }
            }
        }

        private int CalculateSection(int operand1, int operand2, OperatorItem oper)
        {
            switch(oper)
            {
                case OperatorItem.Plus:
                    return operand1 + operand2;
                case OperatorItem.Minus:
                    return operand1 - operand2;
                case OperatorItem.Times:
                    return operand1 * operand2;
                case OperatorItem.Divides:
                    return operand1 / operand2;
                default:
                    throw new Exception("Fail to calculate.");
            }            
        }

        private int CalculatedProcessedElements()
        {
            int len = this.newElementList.Count;
            int index = 0;

            while(index < this.newElementList.Count)
            {
                if (this.newElementList[index] is Operand)
                {
                    index++;
                }
                else
                {
                    int operand1 = ((Operand)this.newElementList[index - 2]).GetNumber();
                    int operand2 = ((Operand)this.newElementList[index - 1]).GetNumber();

                    int subResult = CalculateSection(operand1, operand2, ((Operator)this.newElementList[index]).GetOperator());
                    int startPoint = index - 2;
                    this.newElementList.RemoveAt(index);
                    this.newElementList.RemoveAt(index-1);
                    this.newElementList.RemoveAt(index-2);
                    this.newElementList.Insert(index - 2, new Operand(subResult));
                    index = index - 1;
                }
            }

            Operand result = (Operand)this.newElementList[0];
            return result.GetNumber();
        }

        public int Calculate(List<CalculateElement> list)
        {
            foreach(CalculateElement element in list)
            {
                Preprocess(element);                      
            }

            int result = CalculatedProcessedElements();
            return result;
        }
    }
}
