using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaWeb.Migrations
{
    /// <inheritdoc />
    public partial class SyncModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "moviegenres_fk0",
                table: "moviegenres");

            migrationBuilder.DropForeignKey(
                name: "moviegenres_fk1",
                table: "moviegenres");

            migrationBuilder.DropForeignKey(
                name: "sessions_fk1",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "sessions_fk2",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "tickets_fk1",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "tickets_fk2",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "tickets_fk3",
                table: "tickets");

            migrationBuilder.DropIndex(
                name: "IX_moviegenres_movie_id",
                table: "moviegenres");

            migrationBuilder.AddColumn<int>(
                name: "GenreId1",
                table: "moviegenres",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__moviegen__C6D8E3B2D3B8F1E3",
                table: "moviegenres",
                columns: new[] { "movie_id", "genre_id" });

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

            migrationBuilder.AddForeignKey(
                name: "moviegenres_fk0",
                table: "moviegenres",
                column: "movie_id",
                principalTable: "movies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "moviegenres_fk1",
                table: "moviegenres",
                column: "genre_id",
                principalTable: "genres",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "sessions_fk1",
                table: "sessions",
                column: "movie_id",
                principalTable: "movies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "sessions_fk2",
                table: "sessions",
                column: "hall_id",
                principalTable: "halls",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "tickets_fk1",
                table: "tickets",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "tickets_fk2",
                table: "tickets",
                column: "session_id",
                principalTable: "sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "tickets_fk3",
                table: "tickets",
                column: "seat_id",
                principalTable: "seats",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_moviegenres_genres_GenreId1",
                table: "moviegenres");

            migrationBuilder.DropForeignKey(
                name: "moviegenres_fk0",
                table: "moviegenres");

            migrationBuilder.DropForeignKey(
                name: "moviegenres_fk1",
                table: "moviegenres");

            migrationBuilder.DropForeignKey(
                name: "sessions_fk1",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "sessions_fk2",
                table: "sessions");

            migrationBuilder.DropForeignKey(
                name: "tickets_fk1",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "tickets_fk2",
                table: "tickets");

            migrationBuilder.DropForeignKey(
                name: "tickets_fk3",
                table: "tickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK__moviegen__C6D8E3B2D3B8F1E3",
                table: "moviegenres");

            migrationBuilder.DropIndex(
                name: "IX_moviegenres_GenreId1",
                table: "moviegenres");

            migrationBuilder.DropColumn(
                name: "GenreId1",
                table: "moviegenres");

            migrationBuilder.CreateIndex(
                name: "IX_moviegenres_movie_id",
                table: "moviegenres",
                column: "movie_id");

            migrationBuilder.AddForeignKey(
                name: "moviegenres_fk0",
                table: "moviegenres",
                column: "movie_id",
                principalTable: "movies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "moviegenres_fk1",
                table: "moviegenres",
                column: "genre_id",
                principalTable: "genres",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "sessions_fk1",
                table: "sessions",
                column: "movie_id",
                principalTable: "movies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "sessions_fk2",
                table: "sessions",
                column: "hall_id",
                principalTable: "halls",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "tickets_fk1",
                table: "tickets",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "tickets_fk2",
                table: "tickets",
                column: "session_id",
                principalTable: "sessions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "tickets_fk3",
                table: "tickets",
                column: "seat_id",
                principalTable: "seats",
                principalColumn: "id");
        }
    }
}
