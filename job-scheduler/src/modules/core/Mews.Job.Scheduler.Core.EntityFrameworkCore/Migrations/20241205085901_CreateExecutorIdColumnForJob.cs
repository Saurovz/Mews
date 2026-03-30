using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class CreateExecutorIdColumnForJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExecutorId",
                table: "Job",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("e1cb46c4-1b82-4958-8d83-a8e8b8a568b2"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutorId",
                table: "Job");
        }
    }
}
