using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class RemovedExecutoryTypeNameValueCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Job_StartUtc_State_ExecutorTypeNameValueNew_NameNew",
                table: "Job");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Job_StartUtc_State_ExecutorTypeNameValueNew_NameNew",
                table: "Job",
                columns: new[] { "StartUtc", "State", "ExecutorTypeNameValueNew", "NameNew" },
                filter: "([IsDeleted]=(0))");
        }
    }
}
