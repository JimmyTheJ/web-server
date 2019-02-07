using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VueServer.Models.User;

namespace VueServer.Models
{
    public class Weight
    {
        [Key]
        public int Id { get; set; }

        public decimal Value { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset Created { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public WSUser User { get; set; }
    }
}
