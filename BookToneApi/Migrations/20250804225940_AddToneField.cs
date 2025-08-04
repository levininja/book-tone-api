using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookToneApi.Migrations
{
    /// <inheritdoc />
    public partial class AddToneField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tone",
                table: "BookToneRecommendations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tone",
                table: "BookToneRecommendations");
        }
    }
}
