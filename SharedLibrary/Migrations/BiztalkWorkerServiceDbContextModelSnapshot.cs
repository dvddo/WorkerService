﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharedLibrary;

namespace SharedLibrary.Migrations
{
    [DbContext(typeof(WorkerServiceDbContext))]
    partial class WorkerServiceDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SharedLibrary.Models.Connection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("IPAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InFolder")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsServer")
                        .HasColumnType("bit");

                    b.Property<int>("MessageType")
                        .HasColumnType("int");

                    b.Property<string>("OutFolder")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Connections");
                });

            modelBuilder.Entity("SharedLibrary.Models.DataMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()");

                    b.Property<Guid?>("ConnectionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("InMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OutMessage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionId");

                    b.ToTable("DataMessages");
                });

            modelBuilder.Entity("SharedLibrary.Models.DebugLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newid()");

                    b.Property<Guid?>("ConnectionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Detail")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionId");

                    b.ToTable("DebugLogs");
                });

            modelBuilder.Entity("SharedLibrary.Models.DataMessage", b =>
                {
                    b.HasOne("SharedLibrary.Models.Connection", "Connection")
                        .WithMany()
                        .HasForeignKey("ConnectionId");

                    b.Navigation("Connection");
                });

            modelBuilder.Entity("SharedLibrary.Models.DebugLog", b =>
                {
                    b.HasOne("SharedLibrary.Models.Connection", "Connection")
                        .WithMany()
                        .HasForeignKey("ConnectionId");

                    b.Navigation("Connection");
                });
#pragma warning restore 612, 618
        }
    }
}