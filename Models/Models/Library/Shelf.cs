using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Library
{
    public class Shelf
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("Bookcase")]
        public int BookcaseId { get; set; }

        public Bookcase Bookcase { get; set; }

        public virtual IList<Book> Books { get; set; }

        public bool Validate()
        {
            if (BookcaseId <= 0)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }

            return true;
        }
    }
}
