using System.ComponentModel.DataAnnotations;

namespace SemoSebAzure.Dto
{
    public record ProductRequest(
        string Name,
        IFormFile Image
    );
}
