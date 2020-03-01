using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Models.Library
{
    public class Series
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Number { get; set; }

        public bool Active { get; set; }

        [NotMapped]
        public virtual IList<SeriesItem> SeriesItems { get; set; }
    }
}
