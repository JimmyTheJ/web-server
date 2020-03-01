using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Models.Library
{
    public class BookHasAuthor
    {
        [Key]
        public int Id { get; set; }


        [ForeignKey("Author")]
        public int AuthorId { get; set; }

        [ForeignKey("Book")]
        public int BookId { get; set; }


        public Author Author { get; set; }

        public Book Book { get; set; }
    }
}
