using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class TournamentEvidenceChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordEvidenceChannelId",
                table: "Tournaments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordEvidenceChannelId",
                table: "Tournaments");
        }
    }
}
