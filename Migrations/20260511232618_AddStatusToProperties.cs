using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Booking_System.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Properties");
        }
    }
}
