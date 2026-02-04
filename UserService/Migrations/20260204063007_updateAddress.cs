using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    /// <inheritdoc />
    public partial class updateAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Address_UserId",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "Address",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "Address",
                newName: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Address_UserId",
                table: "Address",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Address_UserId",
                table: "Address");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Address",
                newName: "Province");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Address",
                newName: "District");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Address_UserId",
                table: "Address",
                column: "UserId",
                unique: true);
        }
    }
}
