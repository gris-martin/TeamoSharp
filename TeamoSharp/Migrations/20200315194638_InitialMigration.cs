using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TeamoSharp.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    PlayPostId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordMessageId = table.Column<long>(nullable: false),
                    DiscordChannelId = table.Column<long>(nullable: false),
                    Game = table.Column<string>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    MaxPlayers = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.PlayPostId);
                });

            migrationBuilder.CreateTable(
                name: "PlayMember",
                columns: table => new
                {
                    PlayMemberId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiscordUserId = table.Column<long>(nullable: false),
                    NumPlayers = table.Column<int>(nullable: false),
                    PlayPostId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayMember", x => x.PlayMemberId);
                    table.ForeignKey(
                        name: "FK_PlayMember_Posts_PlayPostId",
                        column: x => x.PlayPostId,
                        principalTable: "Posts",
                        principalColumn: "PlayPostId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayMember_PlayPostId",
                table: "PlayMember",
                column: "PlayPostId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayMember");

            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
