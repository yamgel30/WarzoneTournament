using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class DiscordNotificationToggles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DiscordDmEvidenceApproved",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiscordDmEvidenceRejected",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiscordDmTeamInvitation",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiscordDmPendingInvite",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiscordAnnounceTournamentStart",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiscordAnnounceMatchResult",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DiscordAnnounceNewRegistration",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DiscordDmEvidenceApproved",       table: "SiteSettings");
            migrationBuilder.DropColumn(name: "DiscordDmEvidenceRejected",       table: "SiteSettings");
            migrationBuilder.DropColumn(name: "DiscordDmTeamInvitation",         table: "SiteSettings");
            migrationBuilder.DropColumn(name: "DiscordDmPendingInvite",          table: "SiteSettings");
            migrationBuilder.DropColumn(name: "DiscordAnnounceTournamentStart",  table: "SiteSettings");
            migrationBuilder.DropColumn(name: "DiscordAnnounceMatchResult",      table: "SiteSettings");
            migrationBuilder.DropColumn(name: "DiscordAnnounceNewRegistration",  table: "SiteSettings");
        }
    }
}
