using System;
using System.Collections.Generic;

namespace InvoiceMicroservice.Models
{
	public class InvoiceRequest
	{
		// Customer Information
		public string? CustomerName { get; set; }
		public string? CustomerAddress { get; set; }
		public string? CustomerEmail { get; set; }
		public string? CustomerPhone { get; set; }

		// Seller Information
		public required string SellerCompanyName { get; set; } = "WeDesign Solutions";
		public required string SellerAddress { get; set; } = "131 Eastwood Ave, Randpark Ridge, Johannesburg, 2169";
		public required string SellerEmail { get; set; } = "contact@wedesignsolutions.com";
		public required string SellerPhone { get; set; } = "+27 11 123 4567";

		// Invoice Details
		public required string InvoiceContent { get; set; }
		public DateTime IssueDate { get; set; } = DateTime.UtcNow;
		public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(30);
    	public int InvoiceNumber { get; set; }

		// Items in the invoice
		public List<OrderItem> Items { get; set; } = new List<OrderItem>();
	}

	public class OrderItem
	{
		public required string Name { get; set; }
		public required decimal Price { get; set; }
		public required int Quantity { get; set; }
	}
}

