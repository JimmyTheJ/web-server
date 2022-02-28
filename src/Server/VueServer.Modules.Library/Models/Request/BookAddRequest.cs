using System.Collections.Generic;

namespace VueServer.Modules.Library.Models.Request
{
    public class BookAddRequest
    {
        public Book Book { get; set; }

        public Series Series { get; set; }

        public Bookcase Bookcase { get; set; }

        public Shelf Shelf { get; set; }

        public IList<Author> Authors { get; set; }

        public IList<Genre> Genres { get; set; }
    }
}
