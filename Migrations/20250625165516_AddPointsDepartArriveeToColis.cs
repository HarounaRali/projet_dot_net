using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviLivraison.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsDepartArriveeToColis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdresseArrivee",
                table: "Colis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdresseDepart",
                table: "Colis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LatitudeArrivee",
                table: "Colis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LatitudeDepart",
                table: "Colis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LongitudeArrivee",
                table: "Colis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LongitudeDepart",
                table: "Colis",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdresseArrivee",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "AdresseDepart",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "LatitudeArrivee",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "LatitudeDepart",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "LongitudeArrivee",
                table: "Colis");

            migrationBuilder.DropColumn(
                name: "LongitudeDepart",
                table: "Colis");
        }
    }
}
