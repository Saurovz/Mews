using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Job",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false, defaultValue: new byte[] { 0 }),
                    NameNew = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExecutorTypeNameValueNew = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Team = table.Column<int>(type: "int", nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogVerbosity = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    PeriodValue = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviousSuccessfulStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxExecutionTimeValue = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Options = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdaterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobExecution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExecutorTypeNameValue = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdaterProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobExecution_Job_JobId",
                        column: x => x.JobId,
                        principalTable: "Job",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Job_CreatorProfileId",
                table: "Job",
                column: "CreatorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Job_ExecutorTypeNameValueNew_NameNew_State",
                table: "Job",
                columns: new[] { "ExecutorTypeNameValueNew", "NameNew", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Job_StartUtc_State_ExecutorTypeNameValueNew_NameNew",
                table: "Job",
                columns: new[] { "StartUtc", "State", "ExecutorTypeNameValueNew", "NameNew" },
                filter: "([IsDeleted]=(0))");

            migrationBuilder.CreateIndex(
                name: "IX_Job_State_IsDeleted",
                table: "Job",
                columns: new[] { "State", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Job_UpdaterProfileId",
                table: "Job",
                column: "UpdaterProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_CreatorProfileId",
                table: "JobExecution",
                column: "CreatorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_JobId",
                table: "JobExecution",
                column: "JobId",
                filter: "([State]=(0))");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_JobId_StartUtc",
                table: "JobExecution",
                columns: new[] { "JobId", "StartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_LogFileId",
                table: "JobExecution",
                column: "LogFileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_StartUtc",
                table: "JobExecution",
                column: "StartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_StartUtc_ExecutorTypeNameValue_State",
                table: "JobExecution",
                columns: new[] { "StartUtc", "ExecutorTypeNameValue", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_StartUtc_JobId",
                table: "JobExecution",
                columns: new[] { "StartUtc", "JobId" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_StartUtc_State",
                table: "JobExecution",
                columns: new[] { "StartUtc", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_JobExecution_UpdaterProfileId",
                table: "JobExecution",
                column: "UpdaterProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobExecution");

            migrationBuilder.DropTable(
                name: "Job");
        }
    }
}
