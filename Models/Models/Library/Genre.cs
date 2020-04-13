using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Library
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool Fiction { get; set; }

        public virtual IList<Book> Books { get; set; }
    }
}
