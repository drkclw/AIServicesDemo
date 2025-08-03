using Azure.AI.TextAnalytics;

namespace AzureAIServicesDemo.Models.Language
{
    public class SentimentAnalysisResult
    {
        readonly public int speakerNumber;
        readonly public double offsetInTicks;
        readonly public AnalyzeSentimentResult sentimentResult;

        public SentimentAnalysisResult(int speakerNumber, double offsetInTicks, AnalyzeSentimentResult sentimentResult)
        {
            this.speakerNumber = speakerNumber;
            this.offsetInTicks = offsetInTicks;
            this.sentimentResult = sentimentResult;
        }
    }
}
