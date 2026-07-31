using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CV_mng_sys.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCvDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "AttributeDefinitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AttributeDefinitions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBuiltIn",
                table: "AttributeDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CvDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PositionId = table.Column<int>(type: "integer", nullable: false),
                    CandidateUserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CvDocuments_AspNetUsers_CandidateUserId",
                        column: x => x.CandidateUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CvDocuments_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_Name",
                table: "AttributeDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CvDocuments_CandidateUserId",
                table: "CvDocuments",
                column: "CandidateUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CvDocuments_PositionId_CandidateUserId",
                table: "CvDocuments",
                columns: new[] { "PositionId", "CandidateUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CvDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AttributeDefinitions_Name",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "IsBuiltIn",
                table: "AttributeDefinitions");
        }
    }
}
