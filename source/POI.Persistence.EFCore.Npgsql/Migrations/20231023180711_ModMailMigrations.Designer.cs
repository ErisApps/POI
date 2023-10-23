﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using POI.Persistence.EFCore.Npgsql.Infrastructure;

#nullable disable

namespace POI.Persistence.EFCore.Npgsql.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20231023180711_ModMailMigrations")]
    partial class ModMailMigrations
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("POI.Persistence.Domain.GlobalUserSettings", b =>
                {
                    b.Property<decimal>("DiscordUserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<LocalDate?>("Birthday")
                        .HasColumnType("date");

                    b.Property<string>("ScoreSaberId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("DiscordUserId");

                    b.HasIndex("ScoreSaberId")
                        .IsUnique();

                    b.ToTable("GlobalUserSettings");
                });

            modelBuilder.Entity("POI.Persistence.Domain.LeaderboardEntry", b =>
                {
                    b.Property<string>("ScoreSaberId")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<long>("CountryRank")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Pp")
                        .HasColumnType("double precision");

                    b.HasKey("ScoreSaberId");

                    b.ToTable("LeaderboardEntries");
                });

            modelBuilder.Entity("POI.Persistence.Domain.ServerDependentUserSettings", b =>
                {
                    b.Property<decimal>("DiscordUserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ServerId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Permissions")
                        .HasColumnType("integer");

                    b.HasKey("DiscordUserId", "ServerId");

                    b.ToTable("ServerDependentUserSettings");
                });

            modelBuilder.Entity("POI.Persistence.Domain.ServerSettings", b =>
                {
                    b.Property<decimal>("ServerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("BirthdayRoleId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("EventsChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("ModMailChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("RankUpFeedChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("StarboardChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long?>("StarboardEmojiCount")
                        .HasColumnType("bigint");

                    b.HasKey("ServerId");

                    b.ToTable("ServerSettings");
                });

            modelBuilder.Entity("POI.Persistence.Domain.StarboardMessages", b =>
                {
                    b.Property<decimal>("ServerId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("StarboardMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("ServerId", "ChannelId", "MessageId");

                    b.ToTable("StarboardMessages");
                });
#pragma warning restore 612, 618
        }
    }
}
