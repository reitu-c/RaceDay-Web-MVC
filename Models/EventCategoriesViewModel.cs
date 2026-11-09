namespace RaceDay.Web.Models
{
    // Combines an event with the categories available for that event.
    public class EventCategoriesViewModel
    {
        public EventViewModel Event { get; set; } = new();

        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}

