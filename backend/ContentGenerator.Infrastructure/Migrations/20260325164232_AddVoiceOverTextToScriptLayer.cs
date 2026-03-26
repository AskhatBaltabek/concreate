using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentGenerator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVoiceOverTextToScriptLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoiceOverText",
                table: "Scripts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoiceOverText",
                table: "Scripts");
        }
    }
}
