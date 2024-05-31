using Azure;
using Azure.AI.ContentSafety;

namespace AzureAIServicesDemo.Services
{
    public class ContentSafetyService
    {
        private readonly ContentSafetyClient _contentSafetyClient;
        private readonly BlocklistClient _blocklistClient;
        public ContentSafetyService(ContentSafetyClient contentSafetyClient,
            BlocklistClient blocklistClient)
        {
            _contentSafetyClient = contentSafetyClient;
            _blocklistClient = blocklistClient;
        }
        public async Task<AnalyzeImageResult> AnalyzeImage(Stream imageStream)
        {
            try
            {
                var imageData = await BinaryData.FromStreamAsync(imageStream);
                ContentSafetyImageData image = new ContentSafetyImageData(imageData);

                var request = new AnalyzeImageOptions(image);
                Response<AnalyzeImageResult> response;

                response = await _contentSafetyClient.AnalyzeImageAsync(request);
                return response;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Analyze image failed.\nStatus code: {0}, Error code: {1}, Error message: {2}", ex.Status, ex.ErrorCode, ex.Message);
                throw;
            }
        }

        public async Task<AnalyzeTextResult> AnalyzeText(string text, int outputType)
        {
            try
            {
                var request = new AnalyzeTextOptions(text);
                request.BlocklistNames.Add("DemoBlocklist");
                request.OutputType = outputType == 1 ? AnalyzeTextOutputType.FourSeverityLevels :
                    AnalyzeTextOutputType.EightSeverityLevels;
                Response<AnalyzeTextResult> response = await _contentSafetyClient.AnalyzeTextAsync(
                    request);
                return response;
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("Analyze image failed.\nStatus code: {0}, Error code: {1}, Error message: {2}", ex.Status, ex.ErrorCode, ex.Message);
                throw;
            }
        }
    }
}
