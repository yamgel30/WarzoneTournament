using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class TeamInvitations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeamInvitations",
                columns: table => new
                {
                    Id               = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId           = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedPlayerId  = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByPlayerId= table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status           = table.Column<int>(type: "int", nullable: false),
                    Message          = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt        = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt        = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt        = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy        = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted        = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt        = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInvitations", x => x.Id);
                    table.ForeignKey("FK_TeamInvitations_Teams_TeamId", x => x.TeamId, "Teams", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_TeamInvitations_Players_InvitedPlayerId", x => x.InvitedPlayerId, "Players", "Id");
                    table.ForeignKey("FK_TeamInvitations_Players_InvitedByPlayerId", x => x.InvitedByPlayerId, "Players", "Id");
                });

            migrationBuilder.CreateIndex("IX_TeamInvitations_TeamId", "TeamInvitations", "TeamId");
            migrationBuilder.CreateIndex("IX_TeamInvitations_InvitedPlayerId", "TeamInvitations", "InvitedPlayerId");
            migrationBuilder.CreateIndex("IX_TeamInvitations_Status", "TeamInvitations", "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("TeamInvitations");
        }
    }
}
