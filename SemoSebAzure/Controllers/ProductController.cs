using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using SemoSebAzure.Db;
using SemoSebAzure.Db.Entities;
using SemoSebAzure.Dto;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SemoSebAzure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(
        SebContext _db,
        BlobContainerClient _blobContainerClient,
        ServiceBusClient _serviceBusClient,
        IConfiguration config
    ) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddAsync(
            [FromForm]ProductRequest request
        )
        {
            var p = new Product { Name = request.Name };
            var added = _db.Add(p).Entity;
            await _db.SaveChangesAsync();

            var stream = new MemoryStream();
            await request.Image.CopyToAsync(stream);
            stream.Position = 0;

            var name = Guid.NewGuid().ToString();

            var blobClient =_blobContainerClient.GetBlobClient(name);

            var headers = new BlobHttpHeaders { ContentType = request.Image.ContentType };

            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = headers,
                Metadata = new Dictionary<string, string> {
                    { "product_id", added.Id.ToString() }
                }
            });


            var sender = _serviceBusClient.CreateSender("queue1");

            var message = new ServiceBusMessage(
                JsonSerializer.Serialize(new { ImageName = name, Product = added })
            );

            await sender.SendMessageAsync(message);

            return Created("", added.Id);
        }


        [HttpGet("image/{productId}")]
        public async Task<IActionResult> GetImage([FromRoute] int productId)
        {
            var blobs = _blobContainerClient.GetBlobsAsync();

            var blobClient = await blobs.Select(
                b => _blobContainerClient.GetBlobClient(b.Name)
            ).FirstOrDefaultAsync(bc => bc.GetProperties().Value.Metadata.FirstOrDefault(kvp => kvp.Key == "product_id").Value == productId.ToString());
            if(blobClient is null)
            {
                return NotFound();
            }
            return File(await blobClient.OpenReadAsync(), blobClient.GetProperties().Value.ContentType);
        }
    }
}
