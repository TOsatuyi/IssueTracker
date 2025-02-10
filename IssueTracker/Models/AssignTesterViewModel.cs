using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Models
{
    public class AssignTesterViewModel
    {
        public int IssueId { get; set; }
        public string SelectedTesterId { get; set; }
        public List<SelectListItem> Testers { get; set; } = new List<SelectListItem>();
    }


}
