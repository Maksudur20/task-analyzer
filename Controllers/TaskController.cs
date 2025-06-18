using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Task_Analyzer.Data;
using Task_Analyzer.Models;
using System.Security.Claims;

namespace Task_Analyzer.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ApplicationDbContext db, ILogger<TaskController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get current user id
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Get only tasks for the current user
                IEnumerable<TodoTask> tasks = await _db.Tasks
                    .Where(t => t.UserId == userId)
                    .OrderBy(t => t.Priority)
                    .ToListAsync();

                int total = tasks.Count();
                int completed = 0;
                int pending = 0;
                int overdue = 0;

                List<TodoTask> seriousTasks = new List<TodoTask>();

                foreach (TodoTask t in tasks)
                {
                    TimeSpan difference = t.DueDate - DateTime.Today;
                    int dayGap = (int)difference.TotalDays;

                    if (dayGap <= 1 && !t.IsCompleted) seriousTasks.Add(t);

                    if (t.IsCompleted) completed++;
                    else if (!t.IsCompleted && DateTime.Now > t.DueDate) overdue++;
                    else if (!t.IsCompleted) pending++;
                }

                ViewBag.total = total;
                ViewBag.completed = completed;
                ViewBag.pending = pending;
                ViewBag.overdue = overdue;
                ViewBag.seriousTasks = seriousTasks;

                return View(tasks);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database connection failed");
                return Problem("Unable to connect to database. Please contact administrator.");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoTask model)
        {
            try
            {
                // Assign the current user ID to the task BEFORE validation
                model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(model.UserId))
                {
                    TempData["Error"] = "User authentication issue. Please try logging in again.";
                    return View(model);
                }
                
                // Remove the UserId error from ModelState since we've set it now
                ModelState.Remove("UserId");
                
                // Check for form validity
                if (!ModelState.IsValid)
                {
                    // Log validation errors
                    foreach (var state in ModelState)
                    {
                        if (state.Value.Errors.Count > 0)
                        {
                            _logger.LogWarning($"Validation error in {state.Key}: {state.Value.Errors[0].ErrorMessage}");
                        }
                    }
                    
                    TempData["Error"] = "Please check all required fields and try again.";
                    return View(model);
                }

                // Validate that the due date is not in the past
                if (DateTime.Today > model.DueDate.Date)
                {
                    TempData["Error"] = "Task due date cannot be set to a past date!";
                    return View(model);
                }
                
                // Set creation date to ensure it's accurate
                model.CreatedAt = DateTime.Now;

                _db.Tasks.Add(model);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Task created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new task");
                TempData["Error"] = "An error occurred while creating the task. Please try again.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Get current user id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var task = await _db.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }
            
            // Verify task belongs to current user
            if (task.UserId != userId)
            {
                return Forbid();
            }

            task.IsCompleted = true;
            _db.Update(task);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Get current user id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var task = await _db.Tasks.FindAsync(id);
            
            if (task == null)
            {
                return NotFound();
            }
            
            // Verify task belongs to current user
            if (task.UserId != userId)
            {
                return Forbid();
            }
            
            return View(task);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TodoTask model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            
            // Get current user id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Verify task belongs to current user
            var task = await _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (task == null || task.UserId != userId)
            {
                return Forbid();
            }
            
            if (ModelState.IsValid)
            {
                // Validate that the due date is not in the past
                if (DateTime.Today > model.DueDate.Date)
                {
                    TempData["Error"] = "Task due date cannot be set to a past date!";
                    return View(model);
                }
                
                try
                {
                    // Ensure we're not changing the UserId
                    model.UserId = userId;
                    
                    // Retain original creation date
                    model.CreatedAt = task.CreatedAt;
                    
                    _db.Update(model);
                    await _db.SaveChangesAsync();
                    
                    TempData["Success"] = "Task updated successfully!";
                    return RedirectToAction("Index");
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
            
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            // Get current user id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var task = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
                
            if (task == null)
            {
                return NotFound();
            }
            
            return View(task);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get current user id
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var task = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
                
            if (task == null)
            {
                return NotFound();
            }
            
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
            
            TempData["Success"] = "Task deleted successfully!";
            return RedirectToAction("Index");
        }
        
        private async Task<bool> TaskExists(int id)
        {
            return await _db.Tasks.AnyAsync(t => t.Id == id);
        }
    }
}
