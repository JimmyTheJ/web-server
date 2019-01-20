using System;
using System.ComponentModel.DataAnnotations;

namespace VueServer.Models
{
    public class Notes
    {
        [Key]
        public int Id { get; set; }

        public int Type { get; set; }
        
        public int Priority { get; set; }

        public String Color { get; set; }
        
        public String Text { get; set; }

        public String Title { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset Updated { get; set; }

        public string User { get; set; }
    }
}
