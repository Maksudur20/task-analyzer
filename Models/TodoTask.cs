using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Task_Analyzer.Models
{
    public class TodoTask
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Due Date")]
        [Required(ErrorMessage = "Due Date is required")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(1);

        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5")]
        [Required(ErrorMessage = "Priority is required")]
        public int Priority { get; set; } = 1;

        public bool IsCompleted { get; set; } = false;
        
        // User relationship
        // Don't require UserId in validation since it's filled by the controller
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
