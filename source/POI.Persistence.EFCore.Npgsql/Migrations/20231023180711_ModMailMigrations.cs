using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POI.Persistence.EFCore.Npgsql.Migrations
{
    /// <inheritdoc />
    public partial class ModMailMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ModMailChannelId",
                table: "ServerSettings",
                type: "numeric(20,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModMailChannelId",
                table: "ServerSettings");
        }
    }
}
