using System;
using System.Collections.Generic;

namespace InvoiceModelling.Models
{
    public class InvoiceModel
    {
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public required Address SellerAddress { get; set; }
        public Address? CustomerAddress { get; set; }
        public required List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string? Comments { get; set; }
    }

    public class OrderItem
    {
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public required int Quantity { get; set; }
    }

    public class Address
    {
        public required string CompanyName { get; set; }
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
    }
}
