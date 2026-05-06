using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Booking_System.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GalleryImages",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleMapUrl",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Properties",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewsCount",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GalleryImages",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "GoogleMapUrl",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ReviewsCount",
                table: "Properties");
        }
    }
}
