using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations
{
    /// <inheritdoc />
    public partial class addressFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShipDistrict",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ShipProvince",
                table: "Orders",
                newName: "ShipCountry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShipCountry",
                table: "Orders",
                newName: "ShipProvince");

            migrationBuilder.AddColumn<string>(
                name: "ShipDistrict",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
