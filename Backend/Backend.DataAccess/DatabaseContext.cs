﻿using Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata;


using Backend.Models;
namespace Backend.DataAccess
{
    /// <summary>
    /// SQL Data context   
    /// </summary>
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration config;

        public DbSet<ReceivedCv> ReceivedCvs { get; set; }


        private readonly DbSet<Application> applications;

        /// <summary>
        /// EF Core constructor
        /// </summary>
        public DatabaseContext()
        {
        }

        /// <summary>
        /// EF Core constructor
        /// </summary>
        public DatabaseContext(DbContextOptions options) : base(options)
        {

        }

        /// <summary>
        /// Contrustor used when a configuration is passed
        /// </summary>
        public DatabaseContext(IConfiguration config)
        {
            this.config = config;
        }

        /// <summary>
        /// Configures the data context
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder == null) return;
            base.OnConfiguring(optionsBuilder);

            var databaseName = string.Empty;
            var connectionString = string.Empty;
            if (this.config != null)
            {
                connectionString = config.GetSection("cosmos:ConnectionString").Value;
                databaseName = config.GetSection("cosmos:Database").Value;
                if (string.IsNullOrEmpty(connectionString))
                {
                    databaseName = config["DatabaseName"]?.ToString();
                    connectionString = config["DatabaseConnectionString"]?.ToString();
                }
            }

            if (!string.IsNullOrEmpty(connectionString))
            {
#if DEBUG
                optionsBuilder.EnableSensitiveDataLogging(true);
#endif
                optionsBuilder.UseCosmos(connectionString, databaseName);
            }
        }


        /// <summary>
        /// Seeds the BD
        /// </summary>
        /// <summary>
        /// Seeds the BD
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>()
                .ToContainer("Applications")
                .HasPartitionKey(c => c.Id)
                .HasNoDiscriminator();
            modelBuilder.Entity<ReceivedCv>().ToContainer("cv-sin-procesar");
            modelBuilder.Entity<ReceivedCv>().HasNoDiscriminator();
            modelBuilder.Entity<ReceivedCv>().HasKey(x => x.ReceivedCvId);
            modelBuilder.Entity<ReceivedCv>().HasPartitionKey(x => x.ReceivedCvId);
        }
    }
}