namespace RaceDay.Web.Models;

public class OrganiserCategoriesViewModel
{
    public EventViewModel Event { get; set; } = new();

    public List<CategoryViewModel> Categories { get; set; } = new();
}
