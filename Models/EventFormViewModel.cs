using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    // Holds and validates event information entered by an Organiser.
    public class EventFormViewModel
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(100)]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an event date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please enter the event location.")]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Open";
    }
}
