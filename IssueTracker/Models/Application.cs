using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Models
{
    public class Application
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // Application Name

        public ICollection<Issue>? Issues { get; set; } // Navigation property
    }
}
