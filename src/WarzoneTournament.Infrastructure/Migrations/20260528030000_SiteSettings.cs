using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class SiteSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupportEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DefaultLogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultBannerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultPlacementPointsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DefaultMatchPointThreshold = table.Column<int>(type: "int", nullable: true),
                    DefaultDiscordGuildId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DefaultDiscordAnnouncementChannelId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DefaultDiscordEvidenceChannelId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SiteSettings");
        }
    }
}
