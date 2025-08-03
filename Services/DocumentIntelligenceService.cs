using Azure.AI.DocumentIntelligence;
using AzureAIServicesDemo.Models.DocumentIntelligence;

namespace AzureAIServicesDemo.Services
{
    public class DocumentIntelligenceService
    {
        private DocumentIntelligenceClient _client;

        public DocumentIntelligenceService(DocumentIntelligenceClient client)
        {
            _client = client;
        }

        public async Task<PatientForm> ExtractPatientForm(Stream documentStream)
        {
            PatientForm? patientForm = new();
            try
            {
                BinaryData documentData = await BinaryData.FromStreamAsync(documentStream);
                AnalyzeDocumentContent content = new AnalyzeDocumentContent
                {
                    Base64Source = documentData
                };

                var result = await _client.AnalyzeDocumentAsync(Azure.WaitUntil.Completed, "patient-form", content);

                foreach (AnalyzedDocument document in result.Value.Documents)
                {
                    var dict = document.Fields;
                    patientForm.GivenNames = dict["given_names"].ValueString;
                }
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }

            return patientForm;
        }

        public async Task<Receipt> ExtractReceipt(Stream documentStream)
        {
            Receipt? receipt = new();
            try
            {
                BinaryData documentData = await BinaryData.FromStreamAsync(documentStream);
                AnalyzeDocumentContent content = new AnalyzeDocumentContent
                {
                    Base64Source = documentData
                };

                var result = await _client.AnalyzeDocumentAsync(Azure.WaitUntil.Completed, "prebuilt-receipt", content);

                foreach (AnalyzedDocument document in result.Value.Documents)
                {
                    var dict = document.Fields;
                    receipt.MerchantName = dict["MerchantName"].ValueString;
                    receipt.Subtotal = (float)dict["Subtotal"].ValueCurrency.Amount;
                    receipt.Total = (float)dict["Total"].ValueCurrency.Amount;
                }
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }

            return receipt;
        }

        public async Task<Invoice> ExtractInvoice(Stream documentStream)
        {
            Invoice? invoice = new();
            try
            {
                BinaryData documentData = await BinaryData.FromStreamAsync(documentStream);
                AnalyzeDocumentContent content = new AnalyzeDocumentContent
                {
                    Base64Source = documentData
                };

                var result = await _client.AnalyzeDocumentAsync(Azure.WaitUntil.Completed, "hd-supply-invoice-items", content);

                foreach (AnalyzedDocument document in result.Value.Documents)
                {
                    var dict = document.Fields;

                    var items = document.Fields["Items"].ValueList;
                    invoice.InvoiceItems = GetInvoiceItems(items);
                    //receipt.MerchantName = dict["MerchantName"].ValueString;
                    //receipt.Subtotal = (float)dict["Subtotal"].ValueCurrency.Amount;
                    //receipt.Total = (float)dict["Total"].ValueCurrency.Amount;
                }
            }
            catch (Exception ex)
            {
                var test = ex.Message;
            }

            return invoice;
        }

        private IEnumerable<InvoiceItem> GetInvoiceItems(IReadOnlyList<DocumentField> items)
        {
            List<InvoiceItem> invoiceItems = new();

            foreach (DocumentField item in items)
            {
                var itemDict = item.ValueDictionary;

                invoiceItems.Add(new InvoiceItem
                {
                    ProductCode = itemDict["ProductCode"].ValueString,
                    Description = itemDict["Description"].ValueString,
                    ProductCategory = itemDict["ProductCategory"].ValueString,
                    OrderQty = itemDict["OrderQuantity"].ValueDouble,
                    ShippedQty = itemDict["Quantity"].ValueDouble,
                    UnitPrice = itemDict["UnitPrice"].ValueString,
                    Unit = itemDict["Unit"].ValueString,
                    Amount = itemDict["Amount"].ValueString

                });
            }

            return invoiceItems;
        }
    }
}
