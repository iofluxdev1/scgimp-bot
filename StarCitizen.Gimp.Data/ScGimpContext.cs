using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace StarCitizen.Gimp.Data
{
    public class ScGimpContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DiscordWebhook> DiscordWebhooks { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }

        private bool _configured;

        public ScGimpContext()
        {
            _configured = false;
        }

        public ScGimpContext(string connectionString) : this(GetOptions(connectionString))
        {
        }

        private static DbContextOptions<ScGimpContext> GetOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<ScGimpContext>(), connectionString, b => b.MigrationsAssembly("StarCitizen.Gimp.Web")).Options;
        }

        public ScGimpContext(DbContextOptions<ScGimpContext> options) : base(options)
        {
            _configured = true;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_configured)
            {
                base.OnConfiguring(optionsBuilder);
            }
            else
            {
                IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                IConfigurationRoot configuration = configBuilder
                    .Build();

                optionsBuilder.UseSqlServer(configuration["ScGimpContext"], b => b.MigrationsAssembly("StarCitizen.Gimp.Web"));
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
