using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueServer.Models.Library
{
    public class Author
    {
        public Author() { }

        public Author(Author author)
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

        public virtual IList<BookAuthor> BookAuthors { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    if (!string.IsNullOrWhiteSpace(LastName))
                    {
                        return LastName;
                    }
                }

                if (string.IsNullOrWhiteSpace(LastName))
                {
                    return "";
                }

                return $"{FirstName} {LastName}";
            }
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(LastName))
            {
                return false;
            }

            return true;
        }
    }
}
