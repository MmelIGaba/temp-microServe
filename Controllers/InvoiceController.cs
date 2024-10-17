using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // for [Authorize]
using InvoiceMicroservice.Models;
using iText.Html2pdf;
using System.Linq;
using System.Collections.Generic;
using iText.Kernel.Pdf;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public static class JwtTokenGenerator
{
	public static string GenerateJwtToken()
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my-very-long-secret-key111122233344455566777888999")); 
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, "TestUser"),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim("role", "User") // Custom claim if required
		};

		var token = new JwtSecurityToken(
			issuer: "PDFGenMicroservice", // Must match the issuer in Program.cs
			audience: "PDFGenMicroserviceAPI", 
			claims: claims,
			expires: DateTime.Now.AddMinutes(30), 
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}

namespace InvoiceMicroservice.Controllers
{
	[ApiController]
	[Route("api/invoices")]
	[Authorize]  // Protects all endpoints in this controller
	public class InvoiceController : ControllerBase
	{
		// New endpoint to generate a JWT token
		[HttpGet("token")]
		[AllowAnonymous] // Allow anyone to access this endpoint for token generation
		public IActionResult GenerateToken()
		{
			string token = JwtTokenGenerator.GenerateJwtToken();
			return Ok(new { Token = token });
		}
		
		[HttpPost("generate")]
		public IActionResult GenerateInvoice([FromBody] InvoiceRequest request)
		{
			var validationErrors = ValidateInvoiceRequest(request);
			if (validationErrors.Any())
			{
				return BadRequest(new { Errors = validationErrors });
			}

			try
			{
				byte[] invoiceBytes = Generate(request);
				return File(invoiceBytes, "application/pdf", "Invoice.pdf");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message} - {ex.StackTrace}");
				return StatusCode(500, "An error occurred while generating the invoice.");
			}
		}

		private byte[] Generate(InvoiceRequest request)
		{
			string htmlFilePath = @"C:\Users\Temp\Desktop\document-microservice\temp-microServe\Templates\InvoiceTemplate.html";
			string htmlContent;

			if (!System.IO.File.Exists(htmlFilePath))
			{
				throw new FileNotFoundException("Template file not found at the specified path.", htmlFilePath);
			}

			try
			{
				htmlContent = System.IO.File.ReadAllText(htmlFilePath);
				Console.WriteLine("HTML Content Loaded Successfully:");
				Console.WriteLine(htmlContent);
			}
			catch (Exception ex)
			{
				throw new Exception($"An error occurred while loading the HTML template: {ex.Message}");
			}

			htmlContent = htmlContent
				.Replace("{{SellerCompanyName}}", request.SellerCompanyName)
				.Replace("{{SellerAddress}}", request.SellerAddress)
				.Replace("{{SellerEmail}}", request.SellerEmail)
				.Replace("{{SellerPhone}}", request.SellerPhone)
				.Replace("{{CustomerName}}", request.CustomerName ?? "")
				.Replace("{{CustomerAddress}}", request.CustomerAddress ?? "")
				.Replace("{{CustomerEmail}}", request.CustomerEmail ?? "")
				.Replace("{{CustomerPhone}}", request.CustomerPhone ?? "")
				.Replace("{{InvoiceContent}}", request.InvoiceContent)
				.Replace("{{InvoiceNumber}}", request.InvoiceNumber.ToString());

			string itemRows = string.Join("", request.Items.Select(item =>
				$"<tr><td>{item.Name}</td><td>{item.Quantity}</td><td>{item.Price:C}</td><td>{(item.Quantity * item.Price):C}</td></tr>"));
			htmlContent = htmlContent.Replace("<!-- Placeholder for items, filled dynamically -->", itemRows);

			decimal total = request.Items.Sum(item => item.Price * item.Quantity);
			htmlContent = htmlContent.Replace("{{Total}}", total.ToString("C"));

			using var memoryStream = new MemoryStream();
			using (var pdfWriter = new PdfWriter(memoryStream))
			{
				HtmlConverter.ConvertToPdf(htmlContent, pdfWriter);
			}

			return memoryStream.ToArray();
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
}
