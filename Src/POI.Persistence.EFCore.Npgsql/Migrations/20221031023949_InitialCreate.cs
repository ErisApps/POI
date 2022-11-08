using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace POI.Persistence.EFCore.Npgsql.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountLinks",
                columns: table => new
                {
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ScoreSaberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLinks", x => x.DiscordId);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardEntries",
                columns: table => new
                {
                    ScoreSaberId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CountryRank = table.Column<long>(type: "bigint", nullable: false),
                    Pp = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntries", x => x.ScoreSaberId);
                });

            migrationBuilder.CreateTable(
                name: "ServerDependentUserSettings",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ServerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Permissions = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerDependentUserSettings", x => new { x.UserId, x.ServerId });
                });

            migrationBuilder.CreateTable(
                name: "ServerSettings",
                columns: table => new
                {
                    ServerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RankUpFeedChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    BirthdayRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSettings", x => x.ServerId);
                });

            migrationBuilder.CreateTable(
                name: "GlobalUserSettings",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Birthday = table.Column<LocalDate>(type: "date", nullable: true),
                    AccountLinksDiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalUserSettings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_GlobalUserSettings_AccountLinks_AccountLinksDiscordId",
                        column: x => x.AccountLinksDiscordId,
                        principalTable: "AccountLinks",
                        principalColumn: "DiscordId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountLinks_ScoreSaberId",
                table: "AccountLinks",
                column: "ScoreSaberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalUserSettings_AccountLinksDiscordId",
                table: "GlobalUserSettings",
                column: "AccountLinksDiscordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalUserSettings");

            migrationBuilder.DropTable(
                name: "LeaderboardEntries");

            migrationBuilder.DropTable(
                name: "ServerDependentUserSettings");

            migrationBuilder.DropTable(
                name: "ServerSettings");

            migrationBuilder.DropTable(
                name: "AccountLinks");
        }
    }
}
