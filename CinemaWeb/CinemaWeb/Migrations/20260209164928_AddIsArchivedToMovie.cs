using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "movies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "movies");
        }
    }
}
