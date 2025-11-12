using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaliBotDotNet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Author",
                columns: table => new
                {
                    AuthorID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    IsQuotable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Author", x => x.AuthorID);
                });

            migrationBuilder.CreateTable(
                name: "Reminder",
                columns: table => new
                {
                    ReminderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuthorID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ReminderText = table.Column<string>(type: "TEXT", nullable: false),
                    ReminderTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsReminderDone = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminder", x => x.ReminderID);
                });

            migrationBuilder.CreateTable(
                name: "AlternativeFact",
                columns: table => new
                {
                    AlternativeFactID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorID = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeFact", x => x.AlternativeFactID);
                    table.ForeignKey(
                        name: "FK_AlternativeFact_Author_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Author",
                        principalColumn: "AuthorID");
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    MessageID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    AuthorID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    GuildID = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    DateSent = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_Message_Author_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Author",
                        principalColumn: "AuthorID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeFact_AuthorID",
                table: "AlternativeFact",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Message_AuthorID",
                table: "Message",
                column: "AuthorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternativeFact");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "Reminder");

            migrationBuilder.DropTable(
                name: "Author");
        }
    }
}
