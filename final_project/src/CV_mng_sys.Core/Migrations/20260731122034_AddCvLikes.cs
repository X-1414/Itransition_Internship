using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CV_mng_sys.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCvLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CvLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CvDocumentId = table.Column<int>(type: "integer", nullable: false),
                    RecruiterUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CvLikes_AspNetUsers_RecruiterUserId",
                        column: x => x.RecruiterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CvLikes_CvDocuments_CvDocumentId",
                        column: x => x.CvDocumentId,
                        principalTable: "CvDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CvLikes_CvDocumentId_RecruiterUserId",
                table: "CvLikes",
                columns: new[] { "CvDocumentId", "RecruiterUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CvLikes_RecruiterUserId",
                table: "CvLikes",
                column: "RecruiterUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CvLikes");
        }
    }
}
