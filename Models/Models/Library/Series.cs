using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Models.Library
{
    public class Series
    {
        public Series () { }

        public Series (Series series)
        {
            if (series != null)
            {
                Active = series.Active;
                Name = series.Name;
                Number = series.Number;
            }            
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int Number { get; set; }

        public bool Active { get; set; }

        [NotMapped]
        public virtual IList<Book> Books { get; set; }
    }
}
