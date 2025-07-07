using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobScraperBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chat_id = table.Column<string>(type: "text", nullable: false),
                    work_type = table.Column<int>(type: "integer", nullable: false),
                    limit = table.Column<int>(type: "integer", nullable: false),
                    posted_time = table.Column<int>(type: "integer", nullable: true),
                    keywords = table.Column<string>(type: "text", nullable: false),
                    is_awaiting_for_keywords = table.Column<bool>(type: "boolean", nullable: false),
                    ignore_jobs_found = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ignored_jobs",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<string>(type: "text", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ignored_jobs", x => new { x.user_id, x.job_id });
                    table.ForeignKey(
                        name: "FK_ignored_jobs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_chat_id",
                table: "users",
                column: "chat_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ignored_jobs");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
