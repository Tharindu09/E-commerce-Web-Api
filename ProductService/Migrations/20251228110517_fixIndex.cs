using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductService.Migrations
{
    /// <inheritdoc />
    public partial class fixIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_InventoryId",
                table: "StockReservations");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_InventoryId",
                table: "StockReservations",
                column: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_InventoryId",
                table: "StockReservations");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_InventoryId",
                table: "StockReservations",
                column: "InventoryId",
                unique: true);
        }
    }
}
