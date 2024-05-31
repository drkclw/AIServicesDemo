using Azure.AI.Translation.Text;
using AzureAIServicesDemo.Models.Translation;

namespace AzureAIServicesDemo.Services
{
    public class TranslatorService
    {
        private TextTranslationClient _translationClient;

        public TranslatorService(TextTranslationClient translationClient)
        {
            _translationClient = translationClient;
        }

        public async Task<TranslatorResult> Translate(string text, string targetLanguage = "es")
        {
            var translationResponse = await _translationClient.TranslateAsync(targetLanguage, text);

            var translationResult = new TranslatorResult();
            translationResult.DetectedLanguage = translationResponse.Value.First().DetectedLanguage.Language;
            translationResult.Translations = translationResponse.Value.Select(response => response.Translations.Select(t => t.Text));

            return translationResult;
        }
    }
}
