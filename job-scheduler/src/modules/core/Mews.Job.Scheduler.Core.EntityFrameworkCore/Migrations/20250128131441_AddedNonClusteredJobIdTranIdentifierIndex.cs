using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedNonClusteredJobIdTranIdentifierIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_JobExecution_JobId_TransactionIdentifier]
                ON [dbo].[JobExecution] ([JobId], [TransactionIdentifier])
                WITH (ONLINE = ON);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobExecution_JobId_TransactionIdentifier",
                table: "JobExecution");
        }
    }
}
