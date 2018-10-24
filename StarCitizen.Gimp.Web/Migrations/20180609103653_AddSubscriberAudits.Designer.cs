﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StarCitizen.Gimp.Data;

namespace StarCitizen.Gimp.Web.Migrations
{
    [DbContext(typeof(ScGimpContext))]
    [Migration("20180609103653_AddSubscriberAudits")]
    partial class AddSubscriberAudits
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rtm-30799")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("StarCitizen.Gimp.Data.DiscordWebhook", b =>
                {
                    b.Property<long>("DiscordWebhookId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<DateTimeOffset?>("DeletedAt");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.HasKey("DiscordWebhookId");

                    b.ToTable("DiscordWebhooks");
                });

            modelBuilder.Entity("StarCitizen.Gimp.Data.Notification", b =>
                {
                    b.Property<long>("NotificationId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Body")
                        .IsRequired();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("Items")
                        .IsRequired();

                    b.Property<string>("Medium")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("NotificationType")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("Recipients")
                        .IsRequired();

                    b.HasKey("NotificationId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("StarCitizen.Gimp.Data.Subscriber", b =>
                {
                    b.Property<long>("SubscriberId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<DateTimeOffset?>("DeletedAt");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("SubscriberId");

                    b.ToTable("Subscribers");
                });

            modelBuilder.Entity("StarCitizen.Gimp.Data.SubscriberAudit", b =>
                {
                    b.Property<long>("SubscriberAuditId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Action")
                        .HasMaxLength(200);

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("FormName")
                        .HasMaxLength(50);

                    b.Property<string>("FormVersion")
                        .HasMaxLength(16);

                    b.Property<string>("Headers")
                        .IsRequired()
                        .HasMaxLength(4000);

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasMaxLength(39);

                    b.Property<long>("SubscriberId");

                    b.HasKey("SubscriberAuditId");

                    b.HasIndex("SubscriberId");

                    b.ToTable("SubscriberAudits");
                });

            modelBuilder.Entity("StarCitizen.Gimp.Data.SubscriberAudit", b =>
                {
                    b.HasOne("StarCitizen.Gimp.Data.Subscriber", "Subscriber")
                        .WithMany("SubscriberAudits")
                        .HasForeignKey("SubscriberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
