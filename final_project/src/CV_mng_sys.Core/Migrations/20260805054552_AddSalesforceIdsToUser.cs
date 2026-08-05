using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_mng_sys.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesforceIdsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalesforceAccountId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesforceContactId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesforceAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SalesforceContactId",
                table: "AspNetUsers");
        }
    }
}
