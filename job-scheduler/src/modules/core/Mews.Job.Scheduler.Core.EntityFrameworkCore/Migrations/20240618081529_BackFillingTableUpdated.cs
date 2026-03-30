using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class BackFillingTableUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSuccessfulSyncJobExecutionId",
                table: "BackFillingTask");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulSyncJobId",
                table: "BackFillingTask");

            migrationBuilder.AddColumn<string>(
                name: "SyncedEntities",
                table: "BackFillingTask",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyncedEntities",
                table: "BackFillingTask");

            migrationBuilder.AddColumn<Guid>(
                name: "LastSuccessfulSyncJobExecutionId",
                table: "BackFillingTask",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastSuccessfulSyncJobId",
                table: "BackFillingTask",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
