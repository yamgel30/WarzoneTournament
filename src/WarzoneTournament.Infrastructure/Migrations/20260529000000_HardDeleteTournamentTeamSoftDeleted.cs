using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class HardDeleteTournamentTeamSoftDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Purge soft-deleted TournamentTeam rows left by the old soft-delete unregister.
            // Going forward, UnregisterTeamFromTournamentAsync uses HardRemove so no orphan
            // rows accumulate and re-registration never hits the unique index violation.
            migrationBuilder.Sql("DELETE FROM TournamentTeams WHERE IsDeleted = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No rollback — deleted rows cannot be recovered.
        }
    }
}
