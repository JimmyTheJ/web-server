using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Library
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
    }
}
