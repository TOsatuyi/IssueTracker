using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int IssueId { get; set; }  // Foreign Key to Issue

        [ForeignKey("IssueId")]
        public Issue Issue { get; set; } // Navigation Property

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UserId { get; set; }  // ID of the user who made the comment

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; } // Navigation Property
    }
}
