namespace AzureAIServicesDemo.Models.Speech
{
    public class TranscriptionPhrase
    {
        readonly public int id;
        readonly public string text;
        readonly public string itn;
        readonly public string lexical;
        readonly public int speakerNumber;
        readonly public string offset;
        readonly public double offsetInTicks;

        public TranscriptionPhrase(int id, string text, string itn, string lexical, int speakerNumber, string offset, double offsetInTicks)
        {
            this.id = id;
            this.text = text;
            this.itn = itn;
            this.lexical = lexical;
            this.speakerNumber = speakerNumber;
            this.offset = offset;
            this.offsetInTicks = offsetInTicks;
        }
    }
}
