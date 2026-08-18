using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalogHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoRotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RotationDegrees",
                table: "Photos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Photos_RotationDegrees_Valid",
                table: "Photos",
                sql: "\"RotationDegrees\" IN (0, 90, 180, 270)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Photos_RotationDegrees_Valid",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "RotationDegrees",
                table: "Photos");
        }
    }
}
