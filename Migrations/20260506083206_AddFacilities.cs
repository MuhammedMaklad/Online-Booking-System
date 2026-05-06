using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Booking_System.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Facilities",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Facilities",
                table: "Properties");
        }
    }
}
