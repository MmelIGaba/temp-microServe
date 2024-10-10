using QuestPDF.Helpers;
using InvoiceModelling.Models;

public static class InvoiceDocumentDataSource
{
    private static Random Random = new Random();

    public static InvoiceModel GetInvoiceDetails(DateTime deliveryDate)
    {
        var items = Enumerable
            .Range(1, 8)
            .Select(i => GenerateRandomOrderItem())
            .ToList();

        return new InvoiceModel
        {
            InvoiceNumber = Random.Next(1_000, 10_000),
            IssueDate = DateTime.Now,
            DueDate = deliveryDate.AddDays(7),
            SellerAddress = GetSellerAddress(),
            CustomerAddress = GenerateRandomAddress(),
            Items = items,
            Comments = Placeholders.Paragraph()
        };
    }

    private static OrderItem GenerateRandomOrderItem()
    {
        return new OrderItem
        {
            Name = Placeholders.Label(),
            Price = (decimal)Math.Round(Random.NextDouble() * 100, 2),
            Quantity = Random.Next(1, 10)
        };
    }

    private static Address GetSellerAddress()
    {
        return new Address
        {
            CompanyName = "WeDesign Solutions",
            Street = "131 Eastwood Ave",
            City = "Randpark Ridge",
            State = "Johannesburg",
            Email = "tradecraft.store@wedesignsolutions.com",
            Phone = "011 587 3841"
        };
    }

    private static Address GenerateRandomAddress()
    {
        return new Address
        {
            CompanyName = Placeholders.Name(),
            Street = Placeholders.Label(),
            City = Placeholders.Label(),
            State = Placeholders.Label(),
            Email = Placeholders.Email(),
            Phone = Placeholders.PhoneNumber()
        };
    }
}
