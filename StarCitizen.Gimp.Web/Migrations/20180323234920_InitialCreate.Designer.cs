﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using StarCitizen.Gimp.Data;
using System;

namespace StarCitizen.Gimp.Web.Migrations
{
    [DbContext(typeof(ScGimpContext))]
    [Migration("20180323234920_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("StarCitizen.Gimp.Data.Subscriber", b =>
                {
                    b.Property<long>("SubscriberId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<DateTimeOffset?>("DeletedAt");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("SubscriberId");

                    b.ToTable("Subscribers");
                });
#pragma warning restore 612, 618
        }
    }
}
