using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class JobCreatedUtcIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Job_CreatedUtc",
                table: "Job",
                column: "CreatedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Job_CreatedUtc",
                table: "Job");
        }
    }
}
