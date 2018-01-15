using Newtonsoft.Json;
using TA.Engine.Controllers;
using TA.Engine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace TAService.Controllers
{
    class AnswerJson
    {
        public string answer { get; set; }
        public string status { get; set; }
    }
    public class TASearchController : ApiController
    {
        private IntentController bot = new IntentController();

        // GET api/psasearch?query={query}&userId={userId}
        public string Get([FromUri]string query, [FromUri]string userId = null)
        {
            string answer = bot.Answer(userId, query);
            AnswerJson json = new AnswerJson();
            json.answer = answer;
            json.status = "ok";

            string respStr = JsonConvert.SerializeObject(json);
            return respStr;
        }
    }
}