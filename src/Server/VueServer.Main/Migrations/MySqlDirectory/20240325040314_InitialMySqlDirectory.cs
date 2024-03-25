using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VueServer.Migrations.MySqlDirectory
{
    /// <inheritdoc />
    public partial class InitialMySqlDirectory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerGroupDirectory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Role = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccessFlags = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerGroupDirectory", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServerUserDirectory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Path = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Default = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFlags = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerUserDirectory", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { "directory", "Directory" }
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
