using System;
using System.Collections.Generic;
using Bot.Common.Data;
using Bot.Common.LUEngine.Luis;
using Bot.Common.LUEngines.RulebasedEngine;
using Bot.Common.Utils;

namespace Bot.Common.LUEngines
{
    public class LUController
    {
        private LuisClient luisClient = new LuisClient();

        private static RuleTextStore ruleStore = RuleTextStore.Instance;
     
        public LUInfo Understand(string utterance)
        {
            LUInfo info = this.luisClient.Query(utterance);

            //DateTime startTime = DateTime.Now;
            //string newUtterance = ruleStore.Preprocess(utterance);

            //LUInfo info = this.luisClient.Query(newUtterance); 

            /*
            string ruleBasedIntent = ruleStore.DetermineIntent(utterance);

            if (!string.IsNullOrWhiteSpace(ruleBasedIntent))
            {
                info.Intent.intent = ruleBasedIntent;
                info.Intent.score = 1;
            }

            List<Entity> rulebasedEntities = ruleStore.ExtractSlot(utterance);              
                   
            if (rulebasedEntities.Count > 0)
            {
                info.EntityList.AddRange(rulebasedEntities);
            }

            DateTime endTime = DateTime.Now;

            LogUtils.Log(startTime, endTime, "[LU Service]");
            */
            return info;
        }
    }
}
