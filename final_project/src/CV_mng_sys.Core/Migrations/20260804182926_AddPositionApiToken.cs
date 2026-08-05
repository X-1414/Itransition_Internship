using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_mng_sys.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionApiToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "Positions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "Positions");
        }
    }
}
