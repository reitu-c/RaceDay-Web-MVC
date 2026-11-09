using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    // Represents the information required to enrol in a race category.
    public class EnrolmentViewModel
    {
        [Required]
        public int CategoryID { get; set; }

        [StringLength(100)]
        [Display(Name = "Emergency Contact")]
        public string? EmergencyContact { get; set; }
    }
}

