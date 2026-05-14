using Microsoft.EntityFrameworkCore;
using SemoSebAzure.Db.Entities;

namespace SemoSebAzure.Db
{
    public class SebContext(DbContextOptions o): DbContext(o)
    {
        public DbSet<Product> Products { get; set; }
    }
}
