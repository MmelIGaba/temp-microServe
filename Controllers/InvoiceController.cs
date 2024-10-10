using Microsoft.AspNetCore.Mvc;
using InvoiceMicroservice.Models;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;
// using QuestPDF.Settings; // Ensure this using directive is added

namespace InvoiceMicroservice.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceController : ControllerBase
    {
        [HttpPost("generate")]
        public IActionResult GenerateInvoice([FromBody] InvoiceRequest request)
        {
            // Validate the incoming request
            var validationErrors = ValidateInvoiceRequest(request);
            if (validationErrors.Any())
            {
                return BadRequest(new { Errors = validationErrors });
            }

            // Generate the invoice as a byte array
            byte[] invoiceBytes = Generate(request);

            // Return the invoice as a file for download
            return File(invoiceBytes, "application/pdf", "Invoice.pdf");
        }

        private byte[] Generate(InvoiceRequest request)
        {
            using var stream = new MemoryStream();

            // Create the invoice document
            var document = new InvoiceDocument(request);

            // Render the document into the stream
            document.GeneratePdf(stream);

            // Return the byte array for the PDF
            return stream.ToArray();
        }

        private List<string> ValidateInvoiceRequest(InvoiceRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Invoice request cannot be null.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(request.SellerCompanyName))
                errors.Add("Seller company name is required.");
            if (string.IsNullOrWhiteSpace(request.SellerAddress))
                errors.Add("Seller address is required.");
            if (string.IsNullOrWhiteSpace(request.SellerEmail))
                errors.Add("Seller email is required.");
            if (string.IsNullOrWhiteSpace(request.SellerPhone))
                errors.Add("Seller phone number is required.");
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                errors.Add("Customer name is required.");
            if (request.Items == null || !request.Items.Any())
                errors.Add("At least one item is required in the invoice.");
            else
            {
                foreach (var item in request.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                        errors.Add("Item name is required.");
                    if (item.Quantity <= 0)
                        errors.Add("Item quantity must be greater than zero.");
                    if (item.Price < 0)
                        errors.Add("Item price cannot be negative.");
                }
            }

            return errors;
        }
    }

    // Define the InvoiceDocument class which implements IDocument
    public class InvoiceDocument : IDocument
    {
        private readonly InvoiceRequest Model;

        public InvoiceDocument(InvoiceRequest model)
        {
            Model = model;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Author = "TradeCraft",
            CreationDate = Model.IssueDate,
        };

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(50);

                // Header
                page.Header().Element(ComposeHeader);

                // Content
                page.Content().Element(ComposeContent);

                // Footer with page numbers
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
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Invoice for {Model.CustomerName}").FontSize(20).SemiBold();
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

                row.ConstantItem(100).Height(50).Placeholder(); // Placeholder for future elements (e.g., logo)
            });
        }
        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40)
                     .Background("#F0F0F0")
                     .AlignCenter()
                     .AlignMiddle()
                     .Column(column => // Use a Column to hold multiple items
                     {
                         // Display each item in the invoice
                         foreach (var item in Model.Items)
                         {
                             column.Item().Text($"{item.Quantity} x {item.Name} @ {item.Price:C} = {item.Quantity * item.Price:C}");
                         }

                         // Calculate and display the total
                         decimal total = Model.Items.Sum(item => item.Price * item.Quantity);
                         column.Item().Text($"Total: {total:C}").Bold().FontSize(18);

                         // Optionally add invoice content if provided
                         if (!string.IsNullOrEmpty(Model.InvoiceContent))
                         {
                             column.Item().PaddingTop(10).Text(Model.InvoiceContent);
                         }
                     });
        }

    }
}
