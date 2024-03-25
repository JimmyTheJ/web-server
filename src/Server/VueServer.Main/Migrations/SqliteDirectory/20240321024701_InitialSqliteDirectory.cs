using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VueServer.Migrations.SqliteDirectory
{
    /// <inheritdoc />
    public partial class InitialSqliteDirectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerGroupDirectory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    AccessFlags = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerGroupDirectory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerUserDirectory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    Default = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFlags = table.Column<byte>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerUserDirectory", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ServerSettings",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "Directory_DefaultPathValue", "" },
                    { "Directory_ShouldUseDefaultPath", "0" }
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { "directory", "Directory" }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "ModuleAddOnId", "Name" },
                values: new object[,]
                {
                    { "directory-create", "directory", "Create" },
                    { "directory-delete", "directory", "Delete" },
                    { "directory-move", "directory", "Move" },
                    { "directory-upload", "directory", "Upload" },
                    { "directory-viewer", "directory", "Viewer" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerGroupDirectory");

            migrationBuilder.DropTable(
                name: "ServerUserDirectory");

            migrationBuilder.DeleteData(
                "ServerSettings",
                "Key",
                new string[] { "Directory_DefaultPathValue", "Directory_ShouldUseDefaultPath" });

            migrationBuilder.DeleteData(
                "Modules",
                "id",
                "directory");

            migrationBuilder.DeleteData(
                "Features",
                "id",
                new string[] { "directory-create", "directory-delete", "directory-move", "directory-upload", "directory-viewer" });
        }
    }
}
