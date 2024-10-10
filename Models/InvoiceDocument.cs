using QuestPDF;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InvoiceModelling.Models;
using InvoiceMicroservice.Models;

public class InvoiceDocument : IDocument
{
    private readonly InvoiceModel Model;

    public InvoiceDocument(InvoiceModel model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata()
    {
        return new DocumentMetadata
        {
            Title = $"Invoice #{Model.InvoiceNumber}",
            Author = "WeDesign Solutions",
            CreationDate = Model.IssueDate,
        };
    }

    public DocumentSettings GetSettings()
    {
        return DocumentSettings.Default;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent); // No need for extra arguments
            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }

    private void ComposeHeader(IContainer container)
    {
        var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Invoice #{Model.InvoiceNumber}").Style(titleStyle);

                column.Item().Text(text =>
                {
                    text.Span("Issue date: ").SemiBold();
                    text.Span($"{Model.IssueDate:d}");
                });

                column.Item().Text(text =>
                {
                    text.Span("Due date: ").SemiBold();
                    text.Span($"{Model.DueDate:d}");
                });
            });

            row.ConstantItem(100).Height(50).Placeholder();
        });
    }

    private void ComposeContent(IContainer container) // Removed 'InvoiceRequest request'
    {
        container.PaddingVertical(40)
                 .Background(Colors.Grey.Lighten3)
                 .AlignCenter()
                 .AlignMiddle();

        // Display each item in the invoice (using Model.Items)
        foreach (var item in Model.Items)
        {
            container.Text($"{item.Quantity} x {item.Name} @ {item.Price:C} = {item.Quantity * item.Price:C}");
        }

        // Calculate and display the total
        decimal total = Model.Items.Sum(item => item.Price * item.Quantity);
        container.Text($"Total: {total:C}").Bold().FontSize(18);

        // Optionally add invoice content if provided
        if (!string.IsNullOrEmpty(Model.Comments))
        {
            // Correctly apply padding here
            container.PaddingTop(10).Text(Model.Comments);
        }
    }
}
