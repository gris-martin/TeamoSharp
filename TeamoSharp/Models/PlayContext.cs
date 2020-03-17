using Microsoft.EntityFrameworkCore;

namespace TeamoSharp.Models
{
    public class PlayContext : DbContext
    {
        public DbSet<PlayPost> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=teamo.db");
        }
    }
}
