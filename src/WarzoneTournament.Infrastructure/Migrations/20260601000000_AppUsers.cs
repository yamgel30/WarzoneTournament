using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarzoneTournament.Infrastructure.Migrations
{
    public partial class AppUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id              = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscordId       = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DiscordUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AvatarHash      = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayName     = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email           = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Role            = table.Column<int>(type: "int", nullable: false),
                    PlayerId        = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt       = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt       = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy       = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy       = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted       = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt       = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_DiscordId",
                table: "AppUsers",
                column: "DiscordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_PlayerId",
                table: "AppUsers",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AppUsers");
        }
    }
}
