namespace AzureAIServicesDemo.Models.Vision
{
    public class ImageAnalysis
    {
        public string? Caption { get; set; }
        public IEnumerable<DenseCaption>? DenseCaptions { get; set; }
        public IEnumerable<ImageTag>? Tags { get; set; }
        public IEnumerable<ImageObject>? Objects { get; set; }
        public IEnumerable<ImagePeople>? People { get; set; }
    }
}
