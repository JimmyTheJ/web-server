using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VueServer.Migrations.SqlServerLibrary
{
    public partial class LibraryFirst : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Deceased = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookcases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookcases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fiction = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shelves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookcaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shelves_Bookcases_BookcaseId",
                        column: x => x.BookcaseId,
                        principalTable: "Bookcases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Edition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hardcover = table.Column<bool>(type: "bit", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    Boxset = table.Column<bool>(type: "bit", nullable: false),
                    Loaned = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeriesNumber = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    BookcaseId = table.Column<int>(type: "int", nullable: true),
                    ShelfId = table.Column<int>(type: "int", nullable: true),
                    SeriesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Bookcases_BookcaseId",
                        column: x => x.BookcaseId,
                        principalTable: "Bookcases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Books_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Books_Shelves_ShelfId",
                        column: x => x.ShelfId,
                        principalTable: "Shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Books_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookHasAuthors",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookHasAuthors", x => new { x.BookId, x.AuthorId });
                    table.ForeignKey(
                        name: "FK_BookHasAuthors_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookHasAuthors_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookHasGenres",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookHasGenres", x => new { x.BookId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_BookHasGenres_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookHasGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Fiction", "Name" },
                values: new object[,]
                {
                    { 21, false, "Travel" },
                    { 23, true, "Action and adventure" },
                    { 24, true, "Alternative history" },
                    { 25, true, "Anthology" },
                    { 26, true, "Chick lit" },
                    { 27, true, "Children's" },
                    { 28, true, "Comic book" },
                    { 29, true, "Coming-of-age" },
                    { 30, true, "Crime" },
                    { 31, true, "Drama" },
                    { 32, true, "Fairytale" },
                    { 33, true, "Fantasy" },
                    { 22, false, "True crime" },
                    { 34, true, "Graphic novel" },
                    { 36, true, "Horror" },
                    { 37, true, "Mystery" },
                    { 38, true, "Paranormal romance" },
                    { 39, true, "Picture book" },
                    { 40, true, "Poetry" },
                    { 41, true, "Political thriller" },
                    { 42, true, "Romance" },
                    { 43, true, "Satire" },
                    { 44, true, "Science fiction" },
                    { 45, true, "Short story" },
                    { 46, true, "Suspense" },
                    { 35, true, "Historical fiction" },
                    { 47, true, "Thriller" },
                    { 48, true, "Young adult" },
                    { 19, false, "Science" },
                    { 1, false, "Art" },
                    { 2, false, "Autobiography" },
                    { 3, false, "Biography" },
                    { 4, false, "Book review" },
                    { 5, false, "Cookbook" },
                    { 20, false, "Self help" },
                    { 7, false, "Dictionary" },
                    { 8, false, "Encyclopedia" },
                    { 9, false, "Guide" },
                    { 6, false, "Diary" },
                    { 11, false, "History" },
                    { 12, false, "Journal" },
                    { 13, false, "Math" }
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Fiction", "Name" },
                values: new object[,]
                {
                    { 14, false, "Memoir" },
                    { 15, false, "Prayer" },
                    { 16, false, "Religion, spirituality, and new age" },
                    { 17, false, "Textbook" },
                    { 18, false, "Review" },
                    { 10, false, "Health" }
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { "library", "Library" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookHasAuthors_AuthorId",
                table: "BookHasAuthors",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BookHasGenres_GenreId",
                table: "BookHasGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_BookcaseId",
                table: "Books",
                column: "BookcaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_SeriesId",
                table: "Books",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_ShelfId",
                table: "Books",
                column: "ShelfId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserId",
                table: "Books",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shelves_BookcaseId",
                table: "Shelves",
                column: "BookcaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookHasAuthors");

            migrationBuilder.DropTable(
                name: "BookHasGenres");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Shelves");

            migrationBuilder.DropTable(
                name: "Bookcases");
        }
    }
}
