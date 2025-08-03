namespace AzureAIServicesDemo.Models.DocumentIntelligence
{
    public class InvoiceItem
    {
        public string ProductCode { get; set; }
        public string Description { get; set; }
        public string ProductCategory { get; set; }
        public double? OrderQty { get; set; }
        public double? ShippedQty { get; set; }
        public string UnitPrice { get; set; }
        public string Unit { get; set; }
        public string Amount { get; set; }
    }
}
