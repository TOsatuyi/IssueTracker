using System.Diagnostics;
using System.Security.Claims;
using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IssuesDbContext _context;

        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IssuesDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context; 
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        //Leads to the Issues page
        public IActionResult Issues()
        {
            var allIssues = _context.Issues
                                    .Include(i=> i.Application)
                                    .OrderByDescending(i => i.Id)
                                    .ToList();
            return View(allIssues);
        }
        //This form is being used to Create the issues, but no longer for the edit action.
        [HttpPost]
        [Authorize]
        public IActionResult CreateEditIssueForm(Issue model)
        {
            var issueInDb = _context.Issues.Find(model.Id);
            var user = User;

            if (issueInDb == null)
            {
                return NotFound();
            }

            // Users can only update the Description
            if (!user.IsInRole("Admin"))
            {
                issueInDb.Description = model.Description;
            }
            else // Admins can update everything
            {
                issueInDb.Description = model.Description;
                issueInDb.Severity = model.Severity;
                issueInDb.Status = model.Status;
            }

            _context.SaveChanges();
            return RedirectToAction("Issues");
        }

        //This is used for the Edit Action, Admin only. 
        [Authorize(Roles = "Admin")]
        public IActionResult EditIssue(int? id)
        {
                var issueInDb = _context.Issues.SingleOrDefault(issue => issue.Id == id);
                return View(issueInDb);

        }

        //This is used for the Delete Action
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteIssue(int id)
        {
            var issue = await _context.Issues.SingleOrDefaultAsync(i => i.Id == id);

            if (issue == null || issue.Status != "Fixed")
            {
                _logger.LogError("Issue not found or status is not 'Fixed' for Issue ID: {IssueId}", id);
                return NotFound();
            }

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Issue deleted successfully for Issue ID: {IssueId}", id);

            return RedirectToAction("AdminDashboard");
        }

        //The actual Action for Creating Issues
        public IActionResult CreateEditIssue(Issue model)
        {
            if (model.ApplicationId == 0)
            {
                ModelState.AddModelError("ApplicationId", "Please select a valid application.");
                ViewBag.Applications = new SelectList(_context.Applications, "Id", "Name");
                return View("Issues", model);
            }

            if (model.Id == 0)
            {
                _context.Issues.Add(model);
            }
            else
            {
                _context.Issues.Update(model);
            }

            _context.SaveChanges();

            return RedirectToAction("Issues");
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult ViewIssue(int id)
        {
            var issue = _context.Issues
                                .Include(i => i.Application)
                                .Include(i => i.Comments)
                                    .ThenInclude(c => c.User)
                                .SingleOrDefault(i => i.Id == id);

            if (issue == null)
            {
                return NotFound();
            }
            return View(issue);
        }


        [HttpPost]
        [Authorize(Roles = "Developer,Tester")]
        public async Task<IActionResult> UpdateIssueStatus(int id, string status)
        {
            var issue = await _context.Issues.SingleOrDefaultAsync(i => i.Id == id);

            if (issue == null)
            {
                _logger.LogError("Issue not found with ID: {IssueId}", id);
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validate based on user role
            if (User.IsInRole("Developer") && issue.DeveloperId != userId)
            {
                _logger.LogError("Unauthorized access: Developer {UserId} cannot update Issue ID: {IssueId}", userId, id);
                return Unauthorized();
            }

            if (User.IsInRole("Tester") && issue.TesterId != userId)
            {
                _logger.LogError("Unauthorized access: Tester {UserId} cannot update Issue ID: {IssueId}", userId, id);
                return Unauthorized();
            }



            // Validate status updates based on role
            if (User.IsInRole("Developer") && status != "Ready for Testing")
            {
                _logger.LogError("Invalid status update: Developer can only mark issues as 'Ready for Testing'.");
                return BadRequest("Developers can only mark issues as 'Ready for Testing'.");
            }

            if (User.IsInRole("Tester") && (status != "Fixed" && status != "Reopened"))
            {
                _logger.LogError("Invalid status update: Tester can only mark issues as 'Fixed'.");
                return BadRequest("Testers can only mark issues as 'Fixed' or 'Reopened'.");
            }

            if (User.IsInRole("Developer") && status == "Ready for Testing")
            {
                issue.ReadyForTestingDate = DateTime.Now;
            }
            else if (User.IsInRole("Tester") && status == "Fixed")
            {
                issue.FixedDate = DateTime.Now;
            }
            else if (User.IsInRole("Tester") && status == "Reopened")
            {
                issue.ReopenedDate = DateTime.Now;
            }
            else
            {
                _logger.LogError("Invalid status update for Issue ID: {IssueId}", id);
                return BadRequest("Invalid status update.");
            }

            // Update the issue status
            issue.Status = status;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Issue status updated to {Status} for Issue ID: {IssueId}", status, id);

            // Redirect based on role
            if (User.IsInRole("Developer"))
            {
                return RedirectToAction("DeveloperDashboard");
            }
            else
            {
                return RedirectToAction("TesterDashboard");
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            var issues = _context.Issues
                                .Include(i => i.Application)
                                .Include(i => i.Developer)
                                .Include(i => i.Tester)
                                .OrderByDescending(i => i.Id)
                                .ToList();

            return View(issues);
        }
        // Admin view to manage applications
        [Authorize(Roles = "Admin")]
        public IActionResult ManageApplications()
        {
            var applications = _context.Applications.ToList();
            return View(applications);
        }

        // Admin view to add a new application
        [Authorize(Roles = "Admin")]
        public IActionResult AddApplication()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddApplication(Application model)
        {
            if (ModelState.IsValid)
            {
                _context.Applications.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageApplications");
            }
            return View(model);
        }

        // Admin view to edit an application
        [Authorize(Roles = "Admin")]
        public IActionResult EditApplication(int id)
        {
            var app = _context.Applications.SingleOrDefault(a => a.Id == id);
            if (app == null) return NotFound();
            return View(app);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditApplication(Application model)
        {
            if (ModelState.IsValid)
            {
                _context.Applications.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageApplications");
            }
            return View(model);
        }

        // Admin view to delete an application
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var app = _context.Applications.SingleOrDefault(a => a.Id == id);
            if (app == null) return NotFound();

            _context.Applications.Remove(app);
            await _context.SaveChangesAsync();

            return RedirectToAction("ManageApplications");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult AssignDeveloper(int id)
        {
            _logger.LogInformation("AssignDeveloper called with Issue ID: {IssueId}", id);
            var issue = _context.Issues.SingleOrDefault(i => i.Id == id);

            //if (issue == null || issue.DeveloperAssigned)
            //    return NotFound();
            if (issue == null)
            {
                _logger.LogError("Issue not found with ID: {IssueId}", id);
                return NotFound();
            }

            // List of developers for the Admin to choose from
            var developers = _userManager.GetUsersInRoleAsync("Developer").Result;

            // Log the developers for debugging
            _logger.LogInformation("Developers: {Developers}", string.Join(", ", developers.Select(d => d.UserName)));

            var model = new AssignDeveloperViewModel
            {
                IssueId = issue.Id,
                Developers = developers.Select(d => new SelectListItem
                {
                    Text = d.UserName,
                    Value = d.Id
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AssignDeveloper(AssignDeveloperViewModel model)
        {
            _logger.LogInformation("AssignDeveloper POST called with Issue ID: {IssueId}, Developer ID: {DeveloperId}", model.IssueId, model.SelectedDeveloperId);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid: {ModelState}", ModelState);
                return View(model);
            }

            var issue = _context.Issues.SingleOrDefault(i => i.Id == model.IssueId);

            if (issue == null)
            {
                _logger.LogError("Issue not found or developer already assigned.");
                return NotFound();
            }

            // Log the selected developer ID for debugging
            _logger.LogInformation("Selected Developer ID: {SelectedDeveloperId}", model.SelectedDeveloperId);


            issue.DeveloperId = model.SelectedDeveloperId;
            issue.DeveloperAssigned = true; // Set developer assigned flag to true
            issue.Status = "In Progress";
            issue.AssignedToDeveloperDate = DateTime.Now; // Log assignment time
            _context.SaveChanges();

            _logger.LogInformation("Developer assigned successfully to Issue ID: {IssueId}", model.IssueId);

            return RedirectToAction("AdminDashboard");
            }

            //return View(model);
        //}

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignTester(int id)
        {
            var issue = await _context.Issues
                              .Include(i => i.Developer)
                              .Include(i => i.Tester)
                              .SingleOrDefaultAsync(i => i.Id == id);

            if (issue == null || !issue.DeveloperAssigned || issue.TesterAssigned)
            {
                _logger.LogError("Issue not found or invalid state for Issue ID: {IssueId}", id);
                return NotFound();
            }

            // List of testers for the Admin to choose from
            var testers = await _userManager.GetUsersInRoleAsync("Tester");

            if (testers == null || !testers.Any())
            {
                _logger.LogError("No testers found in the database.");
                return NotFound();
            }

            var model = new AssignTesterViewModel
            {
                IssueId = issue.Id,
                Testers = testers.Select(t => new SelectListItem
                {
                    Text = t.UserName,
                    Value = t.Id
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignTester(AssignTesterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid: {ModelState}", ModelState);
                return View(model);
            }

            var issue = await _context.Issues.SingleOrDefaultAsync(i => i.Id == model.IssueId);

            if (issue == null || issue.TesterAssigned)
            {
                _logger.LogError("Issue not found or tester already assigned for Issue ID: {IssueId}", model.IssueId);
                return NotFound();
            }

            issue.TesterId = model.SelectedTesterId;
            issue.TesterAssigned = true;
            issue.Status = "Undergoing Tests";
            issue.AssignedToTesterDate = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tester assigned successfully to Issue ID: {IssueId}", model.IssueId);

            return RedirectToAction("AdminDashboard");

        }



        //TTTTTTTTEEEEEEEEEEEEEESSSSSSSSSSSSSTTTTTTTTTTTTTTTTTT
        [Authorize(Roles = "Admin")]
        public IActionResult ViewAssignedIssue(int id)
        {
            var issue = _context.Issues
                .Include(i => i.Developer)
                .Include(i => i.Tester)
                .SingleOrDefault(i => i.Id == id);

            if (issue == null)
                return NotFound();

            return View(issue);
        }

        [Authorize(Roles = "Developer")]
        public IActionResult DeveloperDashboard()
        {
            var developerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var issues = _context.Issues
                                        .Include(i => i.Application)
                                        .Where(i => i.DeveloperId == developerId)
                                        .OrderByDescending(i => i.Id)
                                        .ToList();
            return View(issues);
        }

        [Authorize(Roles = "Tester")]
        public async Task<IActionResult> TesterDashboard()
        {
            var testerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var issues = await _context.Issues
                               .Include(i => i.Application)
                               .Include(i => i.Developer)
                               .Where(i => i.TesterId == testerId)
                               .OrderByDescending(i => i.Id)
                               .ToListAsync();
            return View(issues);
        }

        // Action to Post a Comment
        [HttpPost]
        public async Task<IActionResult> AddComment(int issueId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("Comment cannot be empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = new Comment
            {
                IssueId = issueId,
                Text = text,
                UserId = userId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewIssue", "Home", new { id = issueId });
        }


        [Authorize(Roles = "User")]
        public IActionResult UserDashboard()
        {
            var applications = _context.Applications.ToList();

            // Check if the Applications table has any data
            if (applications == null || !applications.Any())
            {
                ViewBag.Applications = new SelectList(new List<string> { "No applications available" });
            }
            else
            {
                ViewBag.Applications = new SelectList(applications, "Id", "Name");
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateIssue(Issue model)
        {
            if (ModelState.IsValid)
            {
                _context.Issues.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Issues");
            }
            return View(model);
        }


        public IActionResult Unauthorized ()
        {
            return View();
        }
    }
}

