using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Library.Models
{
    public class Bookcase
    {
        public Bookcase() { }

        public Bookcase(Bookcase bookcase)
        {
            if (bookcase != null)
            {
                Name = bookcase.Name;
            }
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual IList<Book> Books { get; set; }

        public virtual IList<Shelf> Shelves { get; set; }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }

            return true;
        }
    }
}
