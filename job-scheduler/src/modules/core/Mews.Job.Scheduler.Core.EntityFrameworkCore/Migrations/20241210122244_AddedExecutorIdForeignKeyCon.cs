using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedExecutorIdForeignKeyCon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ExecutorId",
                table: "Job",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValue: new Guid("e1cb46c4-1b82-4958-8d83-a8e8b8a568b2"));

            migrationBuilder.CreateIndex(
                name: "IX_Job_ExecutorId",
                table: "Job",
                column: "ExecutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Executor_ExecutorId",
                table: "Job",
                column: "ExecutorId",
                principalTable: "Executor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Job_Executor_ExecutorId",
                table: "Job");

            migrationBuilder.DropIndex(
                name: "IX_Job_ExecutorId",
                table: "Job");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExecutorId",
                table: "Job",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("e1cb46c4-1b82-4958-8d83-a8e8b8a568b2"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
