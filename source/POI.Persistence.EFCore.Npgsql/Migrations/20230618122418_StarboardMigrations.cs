using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POI.Persistence.EFCore.Npgsql.Migrations
{
    /// <inheritdoc />
    public partial class StarboardMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StarboardChannelId",
                table: "ServerSettings",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StarboardEmojiCount",
                table: "ServerSettings",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StarboardMessages",
                columns: table => new
                {
                    ServerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StarboardMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarboardMessages", x => new { x.ServerId, x.ChannelId, x.MessageId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarboardMessages");

            migrationBuilder.DropColumn(
                name: "StarboardChannelId",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "StarboardEmojiCount",
                table: "ServerSettings");
        }
    }
}
