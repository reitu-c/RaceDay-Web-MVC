namespace RaceDay.Web.Models
{
    // Combines an enrolment with its event and category information.
    public class MyEventViewModel
    {
        public EnrolmentRecordViewModel Enrolment { get; set; } = new();

        public EventViewModel Event { get; set; } = new();

        public CategoryViewModel Category { get; set; } = new();
    }
}

