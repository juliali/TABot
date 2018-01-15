using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Common.Data;
using Bot.Common.Utils;

namespace Bot.Common.LUEngines.RulebasedEngine
{
    public class RuleTextStore
    {
        private static RuleTextStore instance;

        private static List<ExtractorRuleInfo>  slotRules;
        private static List<ExtractorRuleInfo>  intentRules;
        private static List<PreprocessRuleInfo> preprocessRules;

        private RuleTextStore()
        {
            slotRules = FileUtils.ReadExtractorInfo("DoradoBot.Common.Res.ChineseCities.txt");
            intentRules = FileUtils.ReadExtractorInfo("DoradoBot.Common.Res.IntentRules.txt");
            preprocessRules = FileUtils.ReadPreprocessInfo("DoradoBot.Common.Res.Preprocess_replacewords.txt");
        }

        public static RuleTextStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RuleTextStore();
                }
                return instance;
            }
        }

        public List<Entity> ExtractSlot(string utterance)
        {
            List<Entity> entities = new List<Entity>();

            foreach (ExtractorRuleInfo Rule in slotRules)
            {
                List<string> matchedValues = FileUtils.Match(Rule.ValueRegex, utterance);

                if (matchedValues.Count > 0)
                {
                    foreach (string matchedValue in matchedValues)
                    {
                        Entity entity = new Entity();
                        entity.value = matchedValue;
                        entity.type = Rule.Type;

                        if (!string.IsNullOrWhiteSpace(Rule.ResolutionValue))
                        {
                            entity.resolution = Rule.ResolutionValue;
                        }

                        entities.Add(entity);
                    }
                }
            }

            return entities;
        }

        public string DetermineIntent(string utterance)
        {
            foreach (ExtractorRuleInfo Rule in intentRules)
            {
                List<string> matchedValues = FileUtils.Match(Rule.ValueRegex, utterance);

                if (matchedValues.Count > 0)
                {
                    return Rule.Type;
                }
            }

            return null;
        }

        public string Preprocess(string utterance)
        {
            string newUtterance = utterance;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                return newUtterance;
            }

            foreach (PreprocessRuleInfo info in preprocessRules)
            {
                newUtterance = FileUtils.ReplaceRegx(info, newUtterance);
            }

            return newUtterance;
        }
    }
}
