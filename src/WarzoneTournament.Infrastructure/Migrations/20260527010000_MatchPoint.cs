using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MatchPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MatchPointThreshold",
                table: "Tournaments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WinnerTeamId",
                table: "Tournaments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMatchPoint",
                table: "TournamentTeams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchPointThreshold",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "WinnerTeamId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "IsMatchPoint",
                table: "TournamentTeams");
        }
    }
}
