using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaliBotDotNet.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildLanguageModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildLanguageModel",
                columns: table => new
                {
                    GuildID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    BuiltAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildLanguageModel", x => x.GuildID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildLanguageModel");
        }
    }
}
