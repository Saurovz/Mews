using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class DropTransactionIdentifierNewTagNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagNew",
                table: "JobExecution");

            migrationBuilder.DropColumn(
                name: "TransactionIdentifierNew",
                table: "JobExecution");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TagNew",
                table: "JobExecution",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionIdentifierNew",
                table: "JobExecution",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
