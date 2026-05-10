using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Booking_System.Migrations
{
    /// <inheritdoc />
    public partial class FixOwnerIdSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AddOwnerToProperty migration file was empty for some team members,
            // so OwnerId may or may not exist. This guard makes it safe either way.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'Properties')
                    AND name = 'OwnerId'
                )
                BEGIN
                    ALTER TABLE [Properties] ADD [OwnerId] nvarchar(450) NULL;

                    CREATE INDEX [IX_Properties_OwnerId] ON [Properties] ([OwnerId]);

                    ALTER TABLE [Properties] ADD CONSTRAINT [FK_Properties_AspNetUsers_OwnerId]
                        FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
