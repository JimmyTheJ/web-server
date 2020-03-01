using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Models.Library
{
    public class Book
    {
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


        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }

        // FK Genre - Optional
        public int? GenreId { get; set; }

        // FK Bookshelf - Optional
        public int? BookshelfId { get; set; }

        public virtual WSUser User { get; set; }

        [NotMapped]
        public virtual Genre Genre { get; set; }

        [NotMapped]
        public virtual Bookshelf Bookshelf { get; set; }

        [NotMapped]
        public virtual SeriesItem SeriesItem { get; set; }

        [NotMapped]
        public virtual IList<Author> Authors { get; set; }
    }
}
