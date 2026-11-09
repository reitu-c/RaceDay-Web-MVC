namespace RaceDay.Web.Models
{
    // Combines event details, available categories and enrolment information.
    public class EnrolmentPageViewModel
    {
        public EventViewModel Event { get; set; } = new();

        public List<CategoryViewModel> Categories { get; set; } = new();

        public EnrolmentViewModel Enrolment { get; set; } = new();
    }
}

