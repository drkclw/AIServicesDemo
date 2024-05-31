using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AzureAIServicesDemo.Services
{
    public class SpeechService
    {
        private IConfiguration _configuration;

        public SpeechService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SpeechConfig Authenticate()
        {
            var speechRegion = _configuration["speechRegion"];
            var speechKey = _configuration["aiServicesKey"];

            var config = SpeechConfig.FromSubscription(speechKey, speechRegion);
            return config;
        }

        public async Task<string> RecognizeFromMic()
        {
            var speechConfig = Authenticate();
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

            var result = await recognizer.RecognizeOnceAsync();

            return result.Text;
        }

        public async Task TextToSpeech(string phrase, string language)
        {
            var speechConfig = Authenticate();
            speechConfig.SpeechSynthesisLanguage = language;
            using var speechSynthesizer = new SpeechSynthesizer(speechConfig);

            await speechSynthesizer.SpeakTextAsync(phrase);
        }
    }
}
