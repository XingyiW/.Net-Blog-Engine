using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment2.Models
{
    public class Assignment2DataContext: DbContext
    {
        public Assignment2DataContext(DbContextOptions<Assignment2DataContext> options)
           : base(options)
        {

        }

        public DbSet<BadWords> BadWords { get; set; }
        public DbSet<Photos> Photos { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<BlogPosts> BlogPosts { get; set; }
        public DbSet<Comments> Comments { get; set; }
    }
}
