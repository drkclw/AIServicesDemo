namespace AzureAIServicesDemo.Models.Translation
{
    public class TranslatorResult
    {
        public string? DetectedLanguage { get; set; }
        public IEnumerable<IEnumerable<string>>? Translations { get; set; }
    }
}
