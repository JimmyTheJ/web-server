using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VueServer.Models.Library
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool Fiction { get; set; }

        public virtual IList<BookGenre> BookGenres { get; set; }
    }
}
