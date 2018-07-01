using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chaos
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base()
        {


        }

        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {


        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            base.OnConfiguring(optionsBuilder);
        }


        public DbSet<Account> Accounts { get; set; }


        public DbSet<Account2> Accounts2 { get; set; }
    }

}
