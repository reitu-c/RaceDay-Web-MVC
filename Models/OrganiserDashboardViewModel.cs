namespace RaceDay.Web.Models;

public class OrganiserDashboardViewModel
{
    // Total events created by the Organiser.
    public int TotalEvents { get; set; }

    // Events currently accepting enrolments.
    public int OpenEvents { get; set; }

    // Total participant enrolments for the Organiser's events.
    public int TotalEnrolments { get; set; }

    // Results recorded for the Organiser's events.
    public int ResultsRecorded { get; set; }
}
