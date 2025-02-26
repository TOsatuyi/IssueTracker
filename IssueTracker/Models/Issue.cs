using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.Models
{
    public class Issue
    {
        public int Id { get; set; }

        [Required]
        public int ApplicationId { get; set; } // Foreign Key

        [ForeignKey("ApplicationId")]
        public Application Application { get; set; } // Navigation Property

        [Required]
        public string? Description { get; set; }

        [Required]
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? AssignedToDeveloperDate { get; set; }
        public DateTime? ReadyForTestingDate { get; set; }
        public DateTime? AssignedToTesterDate { get; set; }
        public DateTime? FixedDate { get; set; }
        public DateTime? ReopenedDate { get; set; }

        public string? Severity { get; set; }
        public string? Status { get; set; }

        // Add a reference to the user who created the issue
        public string? UserId { get; set; }
        // New fields for Developer and Tester assignment
        public string? DeveloperId { get; set; }
        public string? TesterId { get; set; }

        // Navigation properties to allow access to the User who is assigned
        public virtual IdentityUser Developer { get; set; }
        public virtual IdentityUser Tester { get; set; }
        public bool DeveloperAssigned { get; set; } // New field
        public bool TesterAssigned { get; set; } // New field

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    }

}
