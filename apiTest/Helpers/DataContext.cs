using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using apiTest.Entities;

namespace apiTest.Helpers
{
    public class DataContext: DbContext
    {
        protected readonly IConfiguration Configuration;

        public DataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // sql server config in future
        }

        public DbSet<User> Users { get; set; }
    }
}
