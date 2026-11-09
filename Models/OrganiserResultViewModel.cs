namespace RaceDay.Web.Models;

public class OrganiserResultViewModel
{
    public int ResultID { get; set; }

    public int EnrolmentID { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public TimeSpan? FinishTime { get; set; }

    public int? OverallPosition { get; set; }

    public int? CategoryPosition { get; set; }

    public string ResultStatus { get; set; } = string.Empty;
}
