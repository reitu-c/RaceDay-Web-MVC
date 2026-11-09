using System;

namespace RaceDay.Web.Models
{
    // Represents an event received from the RaceDay REST API.
    public class EventViewModel
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = "Open";
    }
}

