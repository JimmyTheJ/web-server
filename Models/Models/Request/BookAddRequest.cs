using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using VueServer.Models.Models.Library;

namespace VueServer.Models.Models.Request
{
    public class BookAddRequest
    {
        public int? GenreId { get; set; }

        public int? SeriesId { get; set; }

        public int? BookShelfId { get; set; }

        public Book Book { get; set; }

        public Series Series { get; set; }

        public Bookshelf Bookshelf { get; set; }

        public IList<Author> Authors { get; set; }

    }
}
