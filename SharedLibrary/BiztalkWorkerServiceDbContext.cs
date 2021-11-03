using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class WorkerServiceDbContext : DbContext
    {
        //Add-Migration CreateNewDB -context WorkerServiceDbContext
        //Update-Database -context WorkerServiceDbContext

        public WorkerServiceDbContext(DbContextOptions<WorkerServiceDbContext> options) : base(options)
        {
            //Add-Migration CreateNewDB -context WorkerServiceDbContext
            //Update-Database -context WorkerServiceDbContext
        }

        public DbSet<Connection> Connections { get; set; }
        public DbSet<DataMessage> DataMessages { get; set; }

        public DbSet<DebugLog> DebugLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Connection>().ToTable("Connections");
            modelBuilder.Entity<Connection>()
                .Property(b => b.Created)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Connection>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");

            modelBuilder.Entity<DataMessage>().ToTable("DataMessages");
            modelBuilder.Entity<DataMessage>()
                .Property(b => b.Created)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<DataMessage>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");

            modelBuilder.Entity<DebugLog>().ToTable("DebugLogs");
            modelBuilder.Entity<DebugLog>()
                .Property(b => b.Created)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<DebugLog>()
                .Property(b => b.Id)
                .HasDefaultValueSql("newid()");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
    }
}
