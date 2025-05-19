// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SettingsIPT101.Models;
using System.Collections.Generic;

namespace SettingsIPT101.SettData
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Settings> Settings { get; set; }
    }
}
