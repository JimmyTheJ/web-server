using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Models.Library
{
    public class BookAuthor
    {
        public int AuthorId { get; set; }

        public int BookId { get; set; }

        public Author Author { get; set; }

        public Book Book { get; set; }
    }
}
