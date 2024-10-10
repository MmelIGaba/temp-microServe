using QuestPDF.Infrastructure;

namespace InvoiceModelling.Documents
{
    public interface IDocument
    {
        DocumentMetadata GetMetadata();
        DocumentSettings GetSettings();
        void Compose(IDocumentContainer container);
    }
}
