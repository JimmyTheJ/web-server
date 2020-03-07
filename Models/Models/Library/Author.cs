using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Models.Library
{
    public class Author
    {
        public Author () { }

        public Author (Author author)
        {
            if (author != null)
            {
                Deceased = author.Deceased;
                FirstName = author.FirstName;
                LastName = author.LastName;
            }            
        }

        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public bool Deceased { get; set; }

        [NotMapped]
        public virtual IList<Book> Books { get; set; }
    }
}
