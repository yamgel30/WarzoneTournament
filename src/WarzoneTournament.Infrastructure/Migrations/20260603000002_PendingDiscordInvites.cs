using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class PendingDiscordInvites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingDiscordInvites",
                columns: table => new
                {
                    Id                  = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId              = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByPlayerId   = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedDiscordId    = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Message             = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt           = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConsumed          = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt           = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt           = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy           = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy           = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted           = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt           = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingDiscordInvites", x => x.Id);
                    table.ForeignKey("FK_PendingDiscordInvites_Teams_TeamId", x => x.TeamId, "Teams", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_PendingDiscordInvites_Players_InvitedByPlayerId", x => x.InvitedByPlayerId, "Players", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_PendingDiscordInvites_InvitedDiscordId", "PendingDiscordInvites", "InvitedDiscordId");
            migrationBuilder.CreateIndex("IX_PendingDiscordInvites_TeamId_InvitedDiscordId", "PendingDiscordInvites", new[] { "TeamId", "InvitedDiscordId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("PendingDiscordInvites");
        }
    }
}
