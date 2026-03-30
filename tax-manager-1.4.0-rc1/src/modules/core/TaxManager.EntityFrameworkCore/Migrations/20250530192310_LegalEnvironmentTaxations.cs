using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxManager.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class LegalEnvironmentTaxations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LegalEnvironmentTaxation",
                columns: table => new
                {
                    LegalEnvironmentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalEnvironmentTaxation", x => new { x.LegalEnvironmentsId, x.TaxationsId });
                    table.ForeignKey(
                        name: "FK_LegalEnvironmentTaxation_LegalEnvironments_LegalEnvironmentsId",
                        column: x => x.LegalEnvironmentsId,
                        principalTable: "LegalEnvironments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LegalEnvironmentTaxation_Taxations_TaxationsId",
                        column: x => x.TaxationsId,
                        principalTable: "Taxations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalEnvironmentTaxation_TaxationsId",
                table: "LegalEnvironmentTaxation",
                column: "TaxationsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegalEnvironmentTaxation");
        }
    }
}
