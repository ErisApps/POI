using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace POI.Persistence.EFCore.Npgsql.Migrations
{
    /// <inheritdoc />
    public partial class LinkRequestTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "LinkRequestTokens",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(LocalDateTime),
                oldType: "timestamp without time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<LocalDateTime>(
                name: "CreatedAt",
                table: "LinkRequestTokens",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");
        }
    }
}
