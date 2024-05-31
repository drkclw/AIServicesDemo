namespace AzureAIServicesDemo.Models.Language
{
    public class Entity
    {
        public string? Category { get; set; }
        public string? SubCategory { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
    }
}
