using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class BackFilledEntityRemoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackFilledEntity");

            migrationBuilder.DropTable(
                name: "BackFillingTask");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackFillingTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    State = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SyncToUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackFillingTask", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackFilledEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BackFillingTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityTypeName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    LastEntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SyncedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackFilledEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackFilledEntity_BackFillingTask_BackFillingTaskId",
                        column: x => x.BackFillingTaskId,
                        principalTable: "BackFillingTask",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackFilledEntity_BackFillingTaskId",
                table: "BackFilledEntity",
                column: "BackFillingTaskId");
        }
    }
}
