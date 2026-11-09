using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class ResultFormViewModel
{
    public int ResultID { get; set; }

    [Required(ErrorMessage = "Please select an enrolment.")]
    public int EnrolmentID { get; set; }

    public TimeSpan? FinishTime { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than zero.")]
    public int? OverallPosition { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than zero.")]
    public int? CategoryPosition { get; set; }

    [Required(ErrorMessage = "Please select a result status.")]
    public string ResultStatus { get; set; } = "Finished";
}
