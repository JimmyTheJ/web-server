using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.User
{
    public class WSUserProfile
    {
        [Key]
        public int Id { get; set; }

        public string AvatarPath { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual WSUser User { get; set; }
    }
}
