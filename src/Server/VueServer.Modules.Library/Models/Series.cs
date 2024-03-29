﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Library.Models
{
    public class Series
    {
        public Series() { }

        public Series(Series series)
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

        public virtual IList<Book> Books { get; set; }

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
