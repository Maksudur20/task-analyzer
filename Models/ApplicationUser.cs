using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Task_Analyzer.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom user properties here
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        // Registration date
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        // Navigation property for user's tasks
        public virtual ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
    }
}
