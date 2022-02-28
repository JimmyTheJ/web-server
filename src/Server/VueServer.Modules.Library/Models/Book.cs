using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Library.Models
{
    public class Book
    {
        public Book() { }

        public Book(Book book)
        {
            if (book != null)
            {
                Boxset = book.Boxset;
                Edition = book.Edition;
                Hardcover = book.Hardcover;
                IsRead = book.IsRead;
                Loaned = book.Loaned;
                Notes = book.Notes;
                SeriesNumber = book.SeriesNumber;
                PublicationDate = book.PublicationDate;
                SubTitle = book.SubTitle;
                Title = book.Title;
                UserId = book.UserId;
            }
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string SubTitle { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PublicationDate { get; set; }

        public string Edition { get; set; }

        public bool Hardcover { get; set; }

        public bool IsRead { get; set; }

        public bool Boxset { get; set; }

        public bool Loaned { get; set; }

        public string Notes { get; set; }

        /// <summary>
        /// The number in the series if we have a series foreign key
        /// </summary>
        public int SeriesNumber { get; set; }

        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }

        // FK Bookcase - Optional
        public int? BookcaseId { get; set; }

        // FK Shelf - Optional
        public int? ShelfId { get; set; }

        // FK Series - Optional
        public int? SeriesId { get; set; }

        public virtual WSUser User { get; set; }

        public virtual Bookcase Bookcase { get; set; }

        public virtual Series Series { get; set; }

        public virtual Shelf Shelf { get; set; }

        public virtual IList<BookAuthor> BookAuthors { get; set; }

        public virtual IList<BookGenre> BookGenres { get; set; }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(UserId))
            {
                return false;
            }

            return true;
        }

        public Book Clone()
        {
            var newBookAuthors = new List<BookAuthor>();
            if (this.BookAuthors != null)
            {
                foreach (var bookAuthor in this.BookAuthors)
                {
                    newBookAuthors.Add(new BookAuthor()
                    {
                        Author = bookAuthor.Author,
                        AuthorId = bookAuthor.AuthorId,
                        Book = bookAuthor.Book,
                        BookId = bookAuthor.BookId
                    });
                };
            }

            var newBookGenres = new List<BookGenre>();
            if (this.BookGenres != null)
            {
                foreach (var bookGenre in this?.BookGenres)
                {
                    newBookGenres.Add(new BookGenre()
                    {
                        Genre = bookGenre.Genre,
                        GenreId = bookGenre.GenreId,
                        Book = bookGenre.Book,
                        BookId = bookGenre.BookId
                    });
                };
            }

            Bookcase newBookcase = null;
            if (this.Bookcase != null)
            {
                newBookcase = new Bookcase()
                {
                    Books = this.Bookcase.Books,
                    Id = this.Bookcase.Id,
                    Name = this.Bookcase.Name,
                    Shelves = this.Bookcase.Shelves
                };
            };

            Shelf newShelf = null;
            if (this.Shelf != null)
            {
                newShelf = new Shelf()
                {
                    Bookcase = this.Shelf.Bookcase,
                    BookcaseId = this.Shelf.BookcaseId,
                    Books = this.Shelf.Books,
                    Id = this.Shelf.Id,
                    Name = this.Shelf.Name,
                };
            };

            Series newSeries = null;
            if (this.Series != null)
            {
                newSeries = new Series()
                {
                    Active = this.Series.Active,
                    Books = this.Series.Books,
                    Id = this.Series.Id,
                    Name = this.Series.Name,
                    Number = this.Series.Number
                };
            };

            return new Book()
            {
                BookAuthors = newBookAuthors,
                Bookcase = newBookcase,
                BookcaseId = this.BookcaseId,
                BookGenres = newBookGenres,
                Boxset = this.Boxset,
                Edition = this.Edition,
                Hardcover = this.Hardcover,
                Id = this.Id,
                IsRead = this.IsRead,
                Loaned = this.Loaned,
                Notes = this.Notes,
                PublicationDate = this.PublicationDate,
                Series = newSeries,
                SeriesId = this.SeriesId,
                SeriesNumber = this.SeriesNumber,
                Shelf = newShelf,
                ShelfId = this.ShelfId,
                SubTitle = this.SubTitle,
                Title = this.Title,
                User = this.User,
                UserId = this.UserId,
            };
        }
    }
}
