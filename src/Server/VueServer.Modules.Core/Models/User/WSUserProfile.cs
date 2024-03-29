﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueServer.Modules.Core.Models.User
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
