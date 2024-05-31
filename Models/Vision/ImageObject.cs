namespace AzureAIServicesDemo.Models.Vision
{
    public class ImageObject
    {
        public string? Property { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public IEnumerable<ImageObjectTag> Tags { get; set; }
    }
}
