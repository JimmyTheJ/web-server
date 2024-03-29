using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Weight.Models
{
    public class Weights
    {
        [Key]
        public int Id { get; set; }

        public decimal Value { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset Created { get; set; }

        public string Notes { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public WSUser User { get; set; }
    }
}
