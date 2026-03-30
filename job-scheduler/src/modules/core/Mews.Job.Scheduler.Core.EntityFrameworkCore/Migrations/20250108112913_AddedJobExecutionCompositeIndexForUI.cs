using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedJobExecutionCompositeIndexForUI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_JobExecution_ExecutorTypeNameValue_State_StartUtc]
                ON JobExecution ([ExecutorTypeNameValue], [State], [StartUtc])
                INCLUDE ([JobId],[EndUtc],[TransactionIdentifier],[CreatedUtc],[UpdatedUtc],[DeletedUtc],[CreatorProfileId],[UpdaterProfileId],[IsDeleted])
                WITH (ONLINE = ON);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobExecution_ExecutorTypeNameValue_State_StartUtc",
                table: "JobExecution");
        }
    }
}
