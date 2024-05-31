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
    }
}
