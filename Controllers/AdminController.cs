using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Task_Analyzer.Data;
using Task_Analyzer.Models;
using Task_Analyzer.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Task_Analyzer.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Where(u => !u.UserName.Equals("admin@taskanalyzer.com"))
                .ToListAsync();
                
            int totalTasks = await _db.Tasks.CountAsync();
            int completedTasks = await _db.Tasks.CountAsync(t => t.IsCompleted);
            int pendingTasks = totalTasks - completedTasks;
            
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.PendingTasks = pendingTasks;
            
            return View(users);
        }
        
        public async Task<IActionResult> UserTasks(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }
            
            var tasks = await _db.Tasks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Priority)
                .ToListAsync();
                
            ViewBag.UserName = user.UserName;
            ViewBag.UserId = userId;
            
            return View(tasks);
        }
        
        [HttpGet]
        public async Task<IActionResult> EditTask(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var task = await _db.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (task == null)
            {
                return NotFound();
            }
            
            return View(task);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTask(int id, TodoTask model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            
            // Get original task to keep the userId and creation date
            var originalTask = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (originalTask == null)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve the original UserId
                    model.UserId = originalTask.UserId;
                    
                    // Preserve the original creation date
                    model.CreatedAt = originalTask.CreatedAt;
                    
                    _db.Update(model);
                    await _db.SaveChangesAsync();
                    
                    TempData["Success"] = "Task updated successfully!";
                    return RedirectToAction("UserTasks", new { userId = model.UserId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TaskExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> DeleteTask(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var task = await _db.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
                
            if (task == null)
            {
                return NotFound();
            }
            
            return View(task);
        }
        
        [HttpPost, ActionName("DeleteTask")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTaskConfirmed(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            
            if (task == null)
            {
                return NotFound();
            }
            
            string userId = task.UserId;
            
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            
            TempData["Success"] = "Task deleted successfully!";
            return RedirectToAction("UserTasks", new { userId });
        }
        
        private async Task<bool> TaskExists(int id)
        {
            return await _db.Tasks.AnyAsync(t => t.Id == id);
        }
    }
}
