using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedExperimentalIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if the index exists and create it if it does not
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = 'IX_JobExecution_JobId_State_IsDeleted'
                    AND object_id = OBJECT_ID('JobExecution')
                )
                BEGIN
                    CREATE INDEX [IX_JobExecution_JobId_State_IsDeleted]
                    ON [JobExecution] ([JobId], [State], [IsDeleted])
                    WHERE ([IsDeleted] = 0)
                    WITH (ONLINE = ON);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the index
            migrationBuilder.DropIndex(
                name: "IX_JobExecution_JobId_State_IsDeleted",
                table: "JobExecution");
        }
    }
}
