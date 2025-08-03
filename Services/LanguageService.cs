using Azure;
using Azure.AI.Language.Conversations;
using Azure.AI.TextAnalytics;
using Azure.Core;
using AzureAIServicesDemo.Models.Language;
using AzureAIServicesDemo.Models.Speech;
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

        public async Task<SentimentAnalysisResult[]> AnalyzeSentiment(TranscriptionPhrase[] phrases)
        {
            // Create a map of phrase ID to phrase data so we can retrieve it later.
            var phraseData = new Dictionary<int, (int speakerNumber, double offsetInTicks)>();

            // Convert each transcription phrase to a "document" as expected by the sentiment analysis SDK.
            var documentsToSend = phrases.Select(phrase =>
            {
                phraseData.Add(phrase.id, (phrase.speakerNumber, phrase.offsetInTicks));
                return new TextDocumentInput(phrase.id.ToString(), phrase.text);
            }
            );

            // We can only analyze sentiment for 10 documents per request.
            // We cannot use SelectMany here because the lambda returns a Task.
            /*var tasks = documentsToSend.Chunk(10).Select(async chunk =>
            {
                var response = await _textAnalyticsClient.AnalyzeSentimentBatchAsync(chunk);
                return response.Value;
            }
            ).ToArray();
            Task.WhenAll(tasks);
            return tasks.SelectMany(task =>
            {
                var result = task.Result;
                return result.Select(documentResult =>
                {
                    (int speakerNumber, double offsetInTicks) = phraseData[Int32.Parse(documentResult.Id)];
                    return new SentimentAnalysisResult(speakerNumber, offsetInTicks, documentResult);
                });
            }
            ).ToArray();*/
            var chunks = documentsToSend.Chunk(10);
            var sentimentResponses = new List<Response<AnalyzeSentimentResultCollection>>();
            foreach (var chunk in chunks)
            {
                sentimentResponses.Add(await _textAnalyticsClient.AnalyzeSentimentBatchAsync(chunk));
            }
            return sentimentResponses.SelectMany(response =>
            {
                var result = response.Value;
                return result.Select(documentResult =>
                {
                    (int speakerNumber, double offsetInTicks) = phraseData[Int32.Parse(documentResult.Id)];
                    return new SentimentAnalysisResult(speakerNumber, offsetInTicks, documentResult);
                });
            }
            ).ToArray();
        }
        public SentimentConfidenceScores[] GetSentimentScores(SentimentAnalysisResult[] sentimentResults)
        {
            return sentimentResults.OrderBy(x => x.offsetInTicks)
            .Select(result => result.sentimentResult.DocumentSentiment.ConfidenceScores).ToArray();
        }
    }
}
