using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VueServer.Models.User;

namespace VueServer.Models
{
    public class Notes
    {
        [Key]
        public int Id { get; set; }

        public int Type { get; set; }

        public int Priority { get; set; }

        public string Color { get; set; }

        public string Text { get; set; }

        public string Title { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset Updated { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public WSUser User { get; set; }
    }
}
