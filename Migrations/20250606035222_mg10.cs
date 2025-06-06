using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SadikTuranECommerce.Migrations
{
    /// <inheritdoc />
    public partial class mg10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "BoatImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "BoatImages",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "BoatImages");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "BoatImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
