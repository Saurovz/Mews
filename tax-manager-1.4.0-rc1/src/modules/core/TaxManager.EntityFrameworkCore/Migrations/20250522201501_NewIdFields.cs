using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxManager.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class NewIdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubdivisionTaxation_Taxations_TaxationsCode",
                table: "SubdivisionTaxation");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxations_Countries_CountryCode",
                table: "Taxations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Taxations",
                table: "Taxations");

            migrationBuilder.DropIndex(
                name: "IX_Taxations_CountryCode",
                table: "Taxations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubdivisionTaxation",
                table: "SubdivisionTaxation");

            migrationBuilder.DropIndex(
                name: "IX_SubdivisionTaxation_TaxationsCode",
                table: "SubdivisionTaxation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LegalEnvironments",
                table: "LegalEnvironments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Taxations");

            migrationBuilder.DropColumn(
                name: "TaxationsCode",
                table: "SubdivisionTaxation");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Subdivisions");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Taxations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Taxations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TaxationsId",
                table: "SubdivisionTaxation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Subdivisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "LegalEnvironments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Countries",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 1)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Taxations",
                table: "Taxations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubdivisionTaxation",
                table: "SubdivisionTaxation",
                columns: new[] { "SubdivisionsId", "TaxationsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_LegalEnvironments",
                table: "LegalEnvironments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Taxations_Code",
                table: "Taxations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Taxations_CountryId",
                table: "Taxations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubdivisionTaxation_TaxationsId",
                table: "SubdivisionTaxation",
                column: "TaxationsId");

            migrationBuilder.CreateIndex(
                name: "IX_Subdivisions_CountryId",
                table: "Subdivisions",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalEnvironments_Code",
                table: "LegalEnvironments",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subdivisions_Countries_CountryId",
                table: "Subdivisions",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubdivisionTaxation_Taxations_TaxationsId",
                table: "SubdivisionTaxation",
                column: "TaxationsId",
                principalTable: "Taxations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taxations_Countries_CountryId",
                table: "Taxations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subdivisions_Countries_CountryId",
                table: "Subdivisions");

            migrationBuilder.DropForeignKey(
                name: "FK_SubdivisionTaxation_Taxations_TaxationsId",
                table: "SubdivisionTaxation");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxations_Countries_CountryId",
                table: "Taxations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Taxations",
                table: "Taxations");

            migrationBuilder.DropIndex(
                name: "IX_Taxations_Code",
                table: "Taxations");

            migrationBuilder.DropIndex(
                name: "IX_Taxations_CountryId",
                table: "Taxations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubdivisionTaxation",
                table: "SubdivisionTaxation");

            migrationBuilder.DropIndex(
                name: "IX_SubdivisionTaxation_TaxationsId",
                table: "SubdivisionTaxation");

            migrationBuilder.DropIndex(
                name: "IX_Subdivisions_CountryId",
                table: "Subdivisions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LegalEnvironments",
                table: "LegalEnvironments");

            migrationBuilder.DropIndex(
                name: "IX_LegalEnvironments_Code",
                table: "LegalEnvironments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Taxations");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Taxations");

            migrationBuilder.DropColumn(
                name: "TaxationsId",
                table: "SubdivisionTaxation");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Subdivisions");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "LegalEnvironments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Countries");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Taxations",
                type: "char(2)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxationsCode",
                table: "SubdivisionTaxation",
                type: "varchar(40)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Subdivisions",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Taxations",
                table: "Taxations",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubdivisionTaxation",
                table: "SubdivisionTaxation",
                columns: new[] { "SubdivisionsId", "TaxationsCode" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_LegalEnvironments",
                table: "LegalEnvironments",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Taxations_CountryCode",
                table: "Taxations",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_SubdivisionTaxation_TaxationsCode",
                table: "SubdivisionTaxation",
                column: "TaxationsCode");

            migrationBuilder.AddForeignKey(
                name: "FK_SubdivisionTaxation_Taxations_TaxationsCode",
                table: "SubdivisionTaxation",
                column: "TaxationsCode",
                principalTable: "Taxations",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taxations_Countries_CountryCode",
                table: "Taxations",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
