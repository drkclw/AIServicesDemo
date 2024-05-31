using Azure;
using Azure.AI.Language.Conversations;
using Azure.AI.TextAnalytics;
using Azure.Core;
using AzureAIServicesDemo.Models.Language;
using System.Text.Json;

namespace AzureAIServicesDemo.Services
{
    public class LanguageService
    {
        private ConversationAnalysisClient _conversationAnalysisClient;
        private TextAnalyticsClient _textAnalyticsClient;

        public LanguageService(ConversationAnalysisClient conversationAnalysisClient, TextAnalyticsClient textAnalyticsClient)
        {
            _conversationAnalysisClient = conversationAnalysisClient;
            _textAnalyticsClient = textAnalyticsClient;
        }


        public async Task<string> GetIntent(string query)
        {
            string projectName = "azure-ai-convo-conf-demo";
            string deploymentName = "CLUDemoDeployment";

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = query,
                        id = "1",
                        participantId = "1",
                    }
                },
                parameters = new
                {
                    projectName,
                    deploymentName,

                    // Use Utf16CodeUnit for strings in .NET.
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };

            Response response = await _conversationAnalysisClient.AnalyzeConversationAsync(RequestContent.Create(data));
            using JsonDocument result = JsonDocument.Parse(response.ContentStream);
            JsonElement conversationalTaskResult = result.RootElement;
            JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");

            var intents = conversationPrediction.GetProperty("intents").EnumerateArray();
            return intents.First(intent => intent.GetProperty("confidenceScore").GetSingle() > 0.60).GetProperty("category").GetString();
        }

        public async Task<IEnumerable<Entity>> RecognizeEntities(string phrase)
        {
            var response = await _textAnalyticsClient.RecognizeEntitiesAsync(phrase);


            return response.Value.Select(entityResult => new Entity
            {
                Category = entityResult.Category.ToString(),
                SubCategory = entityResult.SubCategory,
                Offset = entityResult.Offset,
                Length = entityResult.Length
            }).OrderBy(entity => entity.Offset);
        }
    }
}
