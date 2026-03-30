using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxManager.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class TaxRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxationTaxRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaxRateId = table.Column<int>(type: "int", nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "char(1)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ValueType = table.Column<string>(type: "varchar(10)", nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StartDateTimeZone = table.Column<string>(type: "varchar(50)", nullable: true),
                    EndDateTimeZone = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxationTaxRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxationTaxRates_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxationTaxRates_Taxations_TaxationId",
                        column: x => x.TaxationId,
                        principalTable: "Taxations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DependentTaxation",
                columns: table => new
                {
                    TaxationTaxRateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildTaxationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependentTaxation", x => new { x.TaxationTaxRateId, x.ChildTaxationId });
                    table.ForeignKey(
                        name: "FK_DependentTaxation_TaxationTaxRates_TaxationTaxRateId",
                        column: x => x.TaxationTaxRateId,
                        principalTable: "TaxationTaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DependentTaxation_Taxations_ChildTaxationId",
                        column: x => x.ChildTaxationId,
                        principalTable: "Taxations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DependentTaxation_ChildTaxationId",
                table: "DependentTaxation",
                column: "ChildTaxationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxationTaxRates_TaxationId",
                table: "TaxationTaxRates",
                column: "TaxationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxationTaxRates_TaxRateId",
                table: "TaxationTaxRates",
                column: "TaxRateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DependentTaxation");

            migrationBuilder.DropTable(
                name: "TaxationTaxRates");

            migrationBuilder.DropTable(
                name: "TaxRates");
        }
    }
}
