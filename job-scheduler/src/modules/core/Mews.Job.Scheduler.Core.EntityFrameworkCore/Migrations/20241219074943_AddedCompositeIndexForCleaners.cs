using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedCompositeIndexForCleaners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Job_StartUtc_State_NameNew",
                table: "Job",
                columns: new[] { "StartUtc", "State", "NameNew" },
                filter: "([IsDeleted]=(0))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Job_StartUtc_State_NameNew",
                table: "Job");
        }
    }
}
