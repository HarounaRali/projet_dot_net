using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviLivraison.Migrations
{
    /// <inheritdoc />
    public partial class AddAdresseLivraisonEtHeureLivraisonEstimee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdresseLivraison",
                table: "Colis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HeureLivraisonEstimee",
                table: "Colis",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdresseLivraison",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "HeureLivraisonEstimee",
                table: "Colis");
        }
    }
}
