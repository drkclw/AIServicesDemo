using Azure.AI.Vision.ImageAnalysis;
using AzureAIServicesDemo.Models.Vision;

namespace AzureAIServicesDemo.Services
{
    public class ImageService
    {
        private ImageAnalysisClient _imageAnalysisClient;

        public ImageService(ImageAnalysisClient imageAnalysisClient)
        {
            _imageAnalysisClient = imageAnalysisClient;
        }

        public async Task<ImageAnalysis> AnalyzeImage(Stream imageStream)
        {
            // Creating a list that defines the features to be extracted from the image. 
            BinaryData imageData = await BinaryData.FromStreamAsync(imageStream);

            VisualFeatures visualFeatures =
                VisualFeatures.Caption |
                VisualFeatures.DenseCaptions |
                VisualFeatures.Objects |
                VisualFeatures.Read |
                VisualFeatures.Tags |
                VisualFeatures.People |
                VisualFeatures.SmartCrops;

            ImageAnalysisOptions options = new ImageAnalysisOptions
            {
                GenderNeutralCaption = false,
                Language = "en",
                SmartCropsAspectRatios = new float[] { 0.9F, 1.33F }
            };

            // Analyze the URL image 
            try
            {
                ImageAnalysisResult results = await _imageAnalysisClient.AnalyzeAsync(imageData, visualFeatures, options);

                return ProcessAnalysisResults(results);
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }

            return null;
        }

        private ImageAnalysis ProcessAnalysisResults(ImageAnalysisResult results)
        {
            var imageAnalysis = new ImageAnalysis();

            imageAnalysis.Caption = results.Caption.Text;

            imageAnalysis.DenseCaptions = results.DenseCaptions.Values.Select(cap => new Models.Vision.DenseCaption
            {
                Text = cap.Text,
                Score = cap.Confidence
            });

            //IMAGE TAGS
            // Image tags and their confidence score
            imageAnalysis.Tags = results.Tags.Values.Select(tag => new Models.Vision.ImageTag
            {
                Name = tag.Name,
                Score = tag.Confidence
            });

            // Objects
            imageAnalysis.Objects = results.Objects.Values.Select(obj => new ImageObject
            {
                Tags = obj.Tags.Select(tag => new ImageObjectTag
                {
                    Score = tag.Confidence,
                    Name = tag.Name
                }),
                X = obj.BoundingBox.X,
                Y = obj.BoundingBox.Y,
                Height = obj.BoundingBox.Height,
                Width = obj.BoundingBox.Width
            });

            //People
            imageAnalysis.People = results.People.Values.Select(people => new ImagePeople
            {
                Score = people.Confidence,
                X = people.BoundingBox.X,
                Y = people.BoundingBox.Y,
                Height = people.BoundingBox.Height,
                Width = people.BoundingBox.Width
            });

            return imageAnalysis;
        }
    }
}
