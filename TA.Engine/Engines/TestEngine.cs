using TA.Engine.Controllers.Component;
using TA.Engine.Data;
using TA.Engine.Services;


namespace TA.Engine.Engines
{
    public class TestEngine
    {
        private CalculatorSrv taskSrv = new CalculatorSrv();

        public readonly string ONEONEPREFIX = "SINGLE_CONTACT";       

        private DBContextManager cManager = new DBContextManager();

        
        public string Answer(string userId, ref TaskFlowContext tfContext)
        {
            string answer = "";
            string userInput = tfContext.UserInput;                        
           
            switch (tfContext.Intent)
            {
                case "DoTest":
                       
                    if (!tfContext.IsInTesting)
                    {
                        answer = this.taskSrv.StartTest(ref tfContext);
                    }
                    else
                    { 
                        if (tfContext.CourseInTest == null)
                        {
                            answer = this.taskSrv.GetCourseInTest(ref tfContext, userInput);                           
                        }
                        else
                        {
                            answer = this.taskSrv.ReceiveUserAnswer(ref tfContext, userInput);                            
                        }
                    }

                    break;                
            }            

            return answer;
        }
    }
}
