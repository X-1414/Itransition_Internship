using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_mng_sys.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscussionPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionPost_AspNetUsers_AuthorUserId",
                table: "DiscussionPost");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionPost_Positions_PositionId",
                table: "DiscussionPost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscussionPost",
                table: "DiscussionPost");

            migrationBuilder.RenameTable(
                name: "DiscussionPost",
                newName: "DiscussionPosts");

            migrationBuilder.RenameIndex(
                name: "IX_DiscussionPost_PositionId",
                table: "DiscussionPosts",
                newName: "IX_DiscussionPosts_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_DiscussionPost_AuthorUserId",
                table: "DiscussionPosts",
                newName: "IX_DiscussionPosts_AuthorUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscussionPosts",
                table: "DiscussionPosts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionPosts_AspNetUsers_AuthorUserId",
                table: "DiscussionPosts",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionPosts_Positions_PositionId",
                table: "DiscussionPosts",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionPosts_AspNetUsers_AuthorUserId",
                table: "DiscussionPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscussionPosts_Positions_PositionId",
                table: "DiscussionPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscussionPosts",
                table: "DiscussionPosts");

            migrationBuilder.RenameTable(
                name: "DiscussionPosts",
                newName: "DiscussionPost");

            migrationBuilder.RenameIndex(
                name: "IX_DiscussionPosts_PositionId",
                table: "DiscussionPost",
                newName: "IX_DiscussionPost_PositionId");

            migrationBuilder.RenameIndex(
                name: "IX_DiscussionPosts_AuthorUserId",
                table: "DiscussionPost",
                newName: "IX_DiscussionPost_AuthorUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscussionPost",
                table: "DiscussionPost",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionPost_AspNetUsers_AuthorUserId",
                table: "DiscussionPost",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscussionPost_Positions_PositionId",
                table: "DiscussionPost",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
