using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Models
{
    public class AssignDeveloperViewModel
    {
        public int IssueId { get; set; }
        public string SelectedDeveloperId { get; set; }
        public List<SelectListItem> Developers { get; set; } = new List<SelectListItem>();
    }
}
