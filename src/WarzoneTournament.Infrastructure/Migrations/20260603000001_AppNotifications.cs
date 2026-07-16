using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class AppNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNotifications",
                columns: table => new
                {
                    Id          = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId      = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title       = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body        = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Link        = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Type        = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsRead      = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt   = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt   = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy   = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy   = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted   = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt   = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppNotifications_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNotifications_UserId",
                table: "AppNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppNotifications_UserId_IsRead",
                table: "AppNotifications",
                columns: new[] { "UserId", "IsRead" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AppNotifications");
        }
    }
}
