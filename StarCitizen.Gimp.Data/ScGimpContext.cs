using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace StarCitizen.Gimp.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ScGimpContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<DiscordWebhook> DiscordWebhooks { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<Subscriber> Subscribers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DbSet<SubscriberAudit> SubscriberAudits { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private readonly bool _configured;

        /// <summary>
        /// 
        /// </summary>
        public ScGimpContext() :
            base()
        {
            _configured = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">The custom SQL server connection string.</param>
        public ScGimpContext(string connectionString) : 
            this(GetSqlServerOptions(connectionString))
        {
            _configured = true;
        }

        /// <summary>
        /// Config used by web app.
        /// </summary>
        /// <param name="options"></param>
        public ScGimpContext(DbContextOptions<ScGimpContext> options) : 
            base(options)
        {
            _configured = true;
        }

        public static void GetSqlServerOptions(SqlServerDbContextOptionsBuilder builder)
        {
            builder
                .MigrationsAssembly("StarCitizen.Gimp.Web")
                .EnableRetryOnFailure
                (
                    5,
                    TimeSpan.FromSeconds(30d),
                    null
                );
        }

        /// <summary>
        /// Config used for console apps and web jobs.
        /// </summary>
        /// <param name="optionsBuilder"></param>
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
                    .AddJsonFile("appsettings.json", true, reloadOnChange: true)
                    .AddEnvironmentVariables();

                IConfigurationRoot configuration = configBuilder
                    .Build();

                optionsBuilder.UseSqlServer
                (
                    configuration["ConnectionStrings:ScGimpContext"], 
                    GetSqlServerOptions
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        private static DbContextOptions<ScGimpContext> GetSqlServerOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions
                .UseSqlServer
                (
                    new DbContextOptionsBuilder<ScGimpContext>(), 
                    connectionString, 
                    GetSqlServerOptions
                )
                .Options;
        }
    }
}
