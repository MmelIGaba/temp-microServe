using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Configure the QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;
// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Simple "Hello World" endpoint for testing
app.MapGet("/hello", () => "Hello, World!");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
