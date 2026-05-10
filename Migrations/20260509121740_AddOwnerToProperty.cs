using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Booking_System.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Properties",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerId",
                table: "Properties",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_OwnerId",
                table: "Properties",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_OwnerId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_OwnerId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Properties");
        }
    }
}
