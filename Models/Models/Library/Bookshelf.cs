using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Models.Library
{
    public class Bookshelf
    {
        public Bookshelf () { }

        public Bookshelf (Bookshelf bookshelf)
        {
            if (bookshelf != null)
                Name = bookshelf.Name;
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public virtual IList<Book> Books { get; set; }
    }
}
