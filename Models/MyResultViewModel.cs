namespace RaceDay.Web.Models
{
    // Combines a race result with its related event information.
    public class MyResultViewModel
    {
        public ResultViewModel Result { get; set; } = new();

        public EnrolmentRecordViewModel Enrolment { get; set; } = new();

        public EventViewModel Event { get; set; } = new();

        public CategoryViewModel Category { get; set; } = new();
    }
}
