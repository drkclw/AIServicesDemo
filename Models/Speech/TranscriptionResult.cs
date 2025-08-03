using AzureAIServicesDemo.Models.Language;

namespace AzureAIServicesDemo.Models.Speech
{
    public class TranscriptionResult
    {
        public TranscriptionPhrase Phrase { get; set; }
        public SentimentAnalysisResult SentimentAnalysis { get; set; }
    }
}
