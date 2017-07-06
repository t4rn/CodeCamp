using System;
using System.ComponentModel.DataAnnotations;

namespace CodeCamp.Api.Models
{
    public class TalkModel
    {
        public string Url { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Abstract { get; set; }
        [Required]
        public string Category { get; set; }
        public string Level { get; set; }
        public string Prerequisites { get; set; }
        public DateTime StartingTime { get; set; } = DateTime.Now;
        public string Room { get; set; }
    }
}
