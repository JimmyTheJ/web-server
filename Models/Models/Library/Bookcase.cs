using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Library
{
    public class Bookcase
    {
        public Bookcase () { }

        public Bookcase (Bookcase bookcase)
        {
            if (bookcase != null)
                Name = bookcase.Name;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual IList<Book> Books { get; set; }

        public virtual IList<Shelf> Shelves { get; set; }
    }
}
