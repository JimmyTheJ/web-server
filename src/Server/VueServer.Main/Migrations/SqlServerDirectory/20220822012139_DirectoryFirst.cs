using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VueServer.Migrations.SqlServerDirectory
{
    public partial class DirectoryFirst : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerGroupDirectory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessFlags = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerGroupDirectory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerUserDirectory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Default = table.Column<bool>(type: "bit", nullable: false),
                    AccessFlags = table.Column<byte>(type: "tinyint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerUserDirectory", x => x.Id);
                });
				
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
                    { "Directory_ShouldUseDefaultPath", "0" },
                    { "Directory_DefaultPathValue", "" }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "ModuleAddOnId", "Name" },
                values: new object[,]
                {
                    { "directory-delete", "directory", "Delete" },
                    { "directory-upload", "directory", "Upload" },
                    { "directory-viewer", "directory", "Viewer" },
                    { "directory-create", "directory", "Create" },
                    { "directory-move", "directory", "Move" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerGroupDirectory");

            migrationBuilder.DropTable(
                name: "ServerUserDirectory");
        }
    }
}
