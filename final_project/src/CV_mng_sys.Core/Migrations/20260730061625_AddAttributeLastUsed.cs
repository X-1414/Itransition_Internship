using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_mng_sys.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeLastUsed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedUtc",
                table: "AttributeDefinitions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUsedUtc",
                table: "AttributeDefinitions");
        }
    }
}
