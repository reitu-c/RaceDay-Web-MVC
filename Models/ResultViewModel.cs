using System;

namespace RaceDay.Web.Models
{
    // Represents a race result received from the REST API.
    public class ResultViewModel
    {
        public int ResultID { get; set; }

        public int EnrolmentID { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        public string ResultStatus { get; set; } = string.Empty;
    }
}

