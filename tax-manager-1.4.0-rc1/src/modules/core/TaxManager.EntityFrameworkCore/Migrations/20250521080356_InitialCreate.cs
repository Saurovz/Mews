using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxManager.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(type: "char(3)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "LegalEnvironments",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(40)", nullable: false),
                    Name = table.Column<string>(type: "varchar(70)", nullable: false),
                    DepositTaxRateMode = table.Column<decimal>(type: "decimal(1,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalEnvironments", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Subdivisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", nullable: false),
                    CountryCode = table.Column<string>(type: "varchar(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subdivisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Taxations",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(40)", nullable: false),
                    CountryCode = table.Column<string>(type: "char(3)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxations", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Taxations_Countries_CountryCode",
                        column: x => x.CountryCode,
                        principalTable: "Countries",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubdivisionTaxation",
                columns: table => new
                {
                    SubdivisionsId = table.Column<int>(type: "int", nullable: false),
                    TaxationsCode = table.Column<string>(type: "varchar(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubdivisionTaxation", x => new { x.SubdivisionsId, x.TaxationsCode });
                    table.ForeignKey(
                        name: "FK_SubdivisionTaxation_Subdivisions_SubdivisionsId",
                        column: x => x.SubdivisionsId,
                        principalTable: "Subdivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubdivisionTaxation_Taxations_TaxationsCode",
                        column: x => x.TaxationsCode,
                        principalTable: "Taxations",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubdivisionTaxation_TaxationsCode",
                table: "SubdivisionTaxation",
                column: "TaxationsCode");

            migrationBuilder.CreateIndex(
                name: "IX_Taxations_CountryCode",
                table: "Taxations",
                column: "CountryCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LegalEnvironments");

            migrationBuilder.DropTable(
                name: "SubdivisionTaxation");

            migrationBuilder.DropTable(
                name: "Subdivisions");

            migrationBuilder.DropTable(
                name: "Taxations");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
