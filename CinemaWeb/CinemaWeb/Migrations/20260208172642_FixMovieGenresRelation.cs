using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixMovieGenresRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_moviegenres_genres_GenreId1",
                table: "moviegenres");

            migrationBuilder.DropIndex(
                name: "IX_moviegenres_GenreId1",
                table: "moviegenres");

            migrationBuilder.DropColumn(
                name: "GenreId1",
                table: "moviegenres");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenreId1",
                table: "moviegenres",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_moviegenres_GenreId1",
                table: "moviegenres",
                column: "GenreId1");

            migrationBuilder.AddForeignKey(
                name: "FK_moviegenres_genres_GenreId1",
                table: "moviegenres",
                column: "GenreId1",
                principalTable: "genres",
                principalColumn: "id");
        }
    }
}
