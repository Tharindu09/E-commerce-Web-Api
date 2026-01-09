using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class secend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "Payments",
                newName: "GatewayPaymentIntentId");

            migrationBuilder.RenameColumn(
                name: "GatewayResponse",
                table: "Payments",
                newName: "GatewayChargeId");

            migrationBuilder.AddColumn<string>(
                name: "Gateway",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayPaymentIntentId",
                table: "Payments",
                column: "GatewayPaymentIntentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_GatewayPaymentIntentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Gateway",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "GatewayPaymentIntentId",
                table: "Payments",
                newName: "PaymentType");

            migrationBuilder.RenameColumn(
                name: "GatewayChargeId",
                table: "Payments",
                newName: "GatewayResponse");
        }
    }
}
