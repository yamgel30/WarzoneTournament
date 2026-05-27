using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiplicadorPorPosicion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename PlacementPoints → PlacementMultiplier
            migrationBuilder.RenameColumn(
                name: "PlacementPoints",
                table: "MatchTeamResults",
                newName: "PlacementMultiplier");

            // Change PlacementMultiplier from int to float (double-precision)
            migrationBuilder.AlterColumn<double>(
                name: "PlacementMultiplier",
                table: "MatchTeamResults",
                type: "float",
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            // Change KillPoints from int to float
            migrationBuilder.AlterColumn<double>(
                name: "KillPoints",
                table: "MatchTeamResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            // Change TotalPoints from int to float
            migrationBuilder.AlterColumn<double>(
                name: "TotalPoints",
                table: "MatchTeamResults",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            // Change TournamentTeams.TotalPoints from int to float
            migrationBuilder.AlterColumn<double>(
                name: "TotalPoints",
                table: "TournamentTeams",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalPoints",
                table: "MatchTeamResults",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "KillPoints",
                table: "MatchTeamResults",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "PlacementMultiplier",
                table: "MatchTeamResults",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 1.0);

            migrationBuilder.RenameColumn(
                name: "PlacementMultiplier",
                table: "MatchTeamResults",
                newName: "PlacementPoints");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPoints",
                table: "TournamentTeams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);
        }
    }
}
