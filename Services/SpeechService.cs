using AzureAIServicesDemo.Helpers;
using AzureAIServicesDemo.Models.Speech;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Net;
using System.Text.Json;

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

        public async Task<string> CreateTranscription(string inputAudioURL, bool useStereoAudio)
        {
            var uri = new UriBuilder(Uri.UriSchemeHttps, "azure-ai-conf-demo.cognitiveservices.azure.com");
            uri.Path = _configuration["speechTranscriptionPath"];

            // Create Transcription API JSON request sample and schema:
            // https://westus.dev.cognitive.microsoft.com/docs/services/speech-to-text-api-v3-0/operations/CreateTranscription
            // Notes:
            // - locale and displayName are required.
            // - diarizationEnabled should only be used with mono audio input.
            var content = new
            {
                contentUrls = new string[] { inputAudioURL },
                properties = new
                {
                    diarizationEnabled = !useStereoAudio,
                    timeToLive = "PT30M"
                },
                locale = _configuration["transcriptionLocale"],
                displayName = $"call_center_{DateTime.Now.ToString()}"
            };
            var response = await RestHelper.SendPost(uri.Uri.ToString(), JsonSerializer.Serialize(content), _configuration["aiServicesKey"]!, new HttpStatusCode[] { HttpStatusCode.Created });
            using (JsonDocument document = JsonDocument.Parse(response.content))
            {
                // Create Transcription API JSON response sample and schema:
                // https://westus.dev.cognitive.microsoft.com/docs/services/speech-to-text-api-v3-0/operations/CreateTranscription
                // NOTE: Remember to call ToString(), EnumerateArray(), GetInt32(), etc. on the return value from GetProperty
                // to convert it from a JsonElement.
                var transcriptionUri = document.RootElement.Clone().GetProperty("self").ToString();
                // The transcription ID is at the end of the transcription URI.
                var transcriptionId = transcriptionUri.Split("/").Last();
                // Verify the transcription ID is a valid GUID.
                Guid guid;
                if (!Guid.TryParse(transcriptionId, out guid))
                {
                    throw new Exception($"Unable to parse response from Create Transcription API:{Environment.NewLine}{response.content}");
                }
                return transcriptionId;
            }
        }

        public string GetTranscriptionUri(JsonElement transcriptionFiles)
        {
            // Get Transcription Files JSON response sample and schema:
            // https://westus.dev.cognitive.microsoft.com/docs/services/speech-to-text-api-v3-0/operations/GetTranscriptionFiles
            return transcriptionFiles.GetProperty("values").EnumerateArray()
                .First(value => 0 == String.Compare("Transcription", value.GetProperty("kind").ToString(), StringComparison.InvariantCultureIgnoreCase))
                .GetProperty("links").GetProperty("contentUrl").ToString();
        }

        public async Task<TranscriptionPhrase[]> GetTranscription(string transcriptionUri)
        {
            var response = await RestHelper.SendGet(transcriptionUri, "", new HttpStatusCode[] { HttpStatusCode.OK });
            using (JsonDocument document = JsonDocument.Parse(response.content))
            {
                var transcription = document.RootElement.Clone();

                // For stereo audio, the phrases are sorted by channel number, so resort them by offset.
                var phrases = transcription
                    .GetProperty("recognizedPhrases").EnumerateArray()
                    .OrderBy(phrase => phrase.GetProperty("offsetInTicks").GetDouble());
                transcription = JsonHelper.AddOrChangeValueInJsonElement<IEnumerable<JsonElement>>(transcription, "recognizedPhrases", phrases);
                return GetTranscriptionPhrases(transcription);
                //SentimentAnalysisResult[] sentimentAnalysisResults = GetSentimentAnalysis(transcriptionPhrases);
                //JsonElement[] sentimentConfidenceScores = GetSentimentConfidenceScores(sentimentAnalysisResults);
                //var conversationItems = TranscriptionPhrasesToConversationItems(transcriptionPhrases);
                // NOTE: Conversation summary is currently in gated public preview. You can sign up here:
                // https://aka.ms/applyforconversationsummarization/
                //var conversationAnalysisUrl = await RequestConversationAnalysis(conversationItems);
                //await WaitForConversationAnalysis(conversationAnalysisUrl);
                //JsonElement conversationAnalysis = await GetConversationAnalysis(conversationAnalysisUrl);


            }
        }

        public async Task WaitForTranscription(string transcriptionId)
        {
            var waitSeconds = 10;
            var done = false;
            while (!done)
            {
                Console.WriteLine($"Waiting {waitSeconds} seconds for transcription to complete.");
                Thread.Sleep(waitSeconds * 1000);
                done = await GetTranscriptionStatus(transcriptionId);
            }
            return;
        }

        private TranscriptionPhrase[] GetTranscriptionPhrases(JsonElement transcription)
        {
            return transcription
                .GetProperty("recognizedPhrases").EnumerateArray()
                .Select((phrase, id) =>
                {
                    var best = phrase.GetProperty("nBest").EnumerateArray().First();
                    // If the user specified stereo audio, and therefore we turned off diarization,
                    // only the channel property is present.
                    // Note: Channels are numbered from 0. Speakers are numbered from 1.
                    int speakerNumber;
                    JsonElement element;
                    if (phrase.TryGetProperty("speaker", out element))
                    {
                        speakerNumber = element.GetInt32() - 1;
                    }
                    else if (phrase.TryGetProperty("channel", out element))
                    {
                        speakerNumber = element.GetInt32();
                    }
                    else
                    {
                        throw new Exception("nBest item contains neither channel nor speaker attribute.");
                    }
                    return new TranscriptionPhrase(id, best.GetProperty("display").ToString(), best.GetProperty("itn").ToString(), best.GetProperty("lexical").ToString(), speakerNumber, phrase.GetProperty("offset").ToString(), phrase.GetProperty("offsetInTicks").GetDouble());
                })
                .ToArray();
        }

        private async Task<bool> GetTranscriptionStatus(string transcriptionId)
        {
            var uri = new UriBuilder(Uri.UriSchemeHttps, "azure-ai-conf-demo.cognitiveservices.azure.com");
            uri.Path = $"{_configuration["speechTranscriptionPath"]}/{transcriptionId}";
            var response = await RestHelper.SendGet(uri.Uri.ToString(), _configuration["aiServicesKey"]!, new HttpStatusCode[] { HttpStatusCode.OK });
            using (JsonDocument document = JsonDocument.Parse(response.content))
            {
                if (0 == string.Compare("Failed", document.RootElement.Clone().GetProperty("status").ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception($"Unable to transcribe audio input. Response:{Environment.NewLine}{response.content}");
                }
                else
                {
                    return 0 == string.Compare("Succeeded", document.RootElement.Clone().GetProperty("status").ToString(), StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }

        public async Task<JsonElement> GetTranscriptionFiles(string transcriptionId)
        {
            var uri = new UriBuilder(Uri.UriSchemeHttps, "azure-ai-conf-demo.cognitiveservices.azure.com");
            uri.Path = $"{_configuration["speechTranscriptionPath"]}/{transcriptionId}/files";
            var response = await RestHelper.SendGet(uri.Uri.ToString(), _configuration["aiServicesKey"]!, new HttpStatusCode[] { HttpStatusCode.OK });
            using (JsonDocument document = JsonDocument.Parse(response.content))
            {
                return document.RootElement.Clone();
            }
        }
    }
}
