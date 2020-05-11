using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VueServer.Models.Directory
{
    public class TranscodeFile
    {
        [Key]
        public int Id { get; set; }

        public string BaseName { get; set; }

        public string TempFileName { get; set; }
    }
}
