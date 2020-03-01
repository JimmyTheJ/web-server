using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Models.Library
{
    public class SeriesItem
    {
        [Key]
        public int Id { get; set; }

        public int Number { get; set; }

        [ForeignKey("Series")]
        public int SeriesId { get; set; }

        [ForeignKey("Book")]
        public int BookId { get; set; }

        public Series Series { get; set; }

        public Book Book { get; set; }
    }
}
