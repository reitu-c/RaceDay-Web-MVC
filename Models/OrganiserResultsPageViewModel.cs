namespace RaceDay.Web.Models;

public class OrganiserResultsPageViewModel
{
    public List<OrganiserResultViewModel> Results { get; set; } = new();

    public List<OrganiserEnrolmentViewModel> Enrolments { get; set; } = new();
}
