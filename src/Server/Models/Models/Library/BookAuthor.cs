namespace VueServer.Models.Library
{
    public class BookAuthor
    {
        public int AuthorId { get; set; }

        public int BookId { get; set; }

        public Author Author { get; set; }

        public Book Book { get; set; }
    }
}
