using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SemoSebAzure.Db;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<SebContext>(
    b => b.UseSqlServer(builder.Configuration.GetConnectionString("Main"))
);

builder.Services.AddScoped(_ => new BlobContainerClient(builder.Configuration.GetConnectionString("Blob"), "images"));

builder.Services.AddScoped(_ => new ServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus")));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthorization();

app.MapControllers();

app.Run();

