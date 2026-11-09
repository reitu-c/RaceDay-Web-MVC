using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorization("Organiser")]
public class OrganiserController : Controller
{
    private readonly RaceDayApiService _apiService;

    public OrganiserController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    // Display dashboard statistics for the logged-in Organiser.
    public async Task<IActionResult> Dashboard()
    {
        // Retrieve the Organiser's events from the API.
        var events = await _apiService.GetAsync<List<EventViewModel>>(
            "api/Events");

        // Only include events created by the logged-in Organiser.
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        var organiserEvents = events?
            .Where(e => e.OrganiserID == organiserId)
            .ToList() ?? new List<EventViewModel>();

        // Retrieve participant enrolments for the Organiser.
        var enrolments =
            await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                "api/Enrolments/organiser");

        // Retrieve all race results.
        var results =
            await _apiService.GetAsync<List<ResultViewModel>>(
                "api/Results");

        // Match results to the Organiser's enrolments.
        var organiserEnrolmentIds = enrolments?
            .Select(e => e.EnrolmentID)
            .ToHashSet() ?? new HashSet<int>();

        var dashboard = new OrganiserDashboardViewModel
        {
            TotalEvents = organiserEvents.Count,

            OpenEvents = organiserEvents.Count(
                e => e.Status == "Open"),

            TotalEnrolments = enrolments?.Count ?? 0,

            ResultsRecorded = results?
                .Count(r => organiserEnrolmentIds.Contains(r.EnrolmentID)) ?? 0
        };

        return View(dashboard);
    }

    // Retrieve events from the Part 2 API.
    public async Task<IActionResult> Events()
    {
        var events = await _apiService.GetAsync<List<EventViewModel>>(
            "api/Events");

        if (events == null)
        {
            ViewBag.ErrorMessage =
                "Unable to retrieve events. Please try again.";

            return View(new List<EventViewModel>());
        }

        // Display only events belonging to the logged-in Organiser.
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        var organiserEvents = events
            .Where(e => e.OrganiserID == organiserId)
            .ToList();

        return View(organiserEvents);
    }
    // Display the Create Event form.
    [HttpGet]
    public IActionResult CreateEvent()
    {
        return View(new EventFormViewModel
        {
            EventDate = DateTime.Today.AddDays(1),
            Status = "Open"
        });
    }

    // Submit a new event to the Part 2 API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormViewModel model)
    {
        if (model.EventDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                "EventDate",
                "The event date cannot be in the past.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var request = new
        {
            EventName = model.EventName,
            EventDate = model.EventDate,
            Location = model.Location,
            Description = model.Description,
            Status = model.Status
        };

        var response = await _apiService.PostForStringAsync(
            "api/Events", request);

        if (response == null || response.StartsWith("API ERROR"))
        {
            ModelState.AddModelError(
                "",
                response ?? "Unable to create the event.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Your event has been created successfully!";

        return RedirectToAction("Events");
    }
    // Display the form for editing an existing event.
    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        // Retrieve the selected event from the API.
        var existingEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{id}");

        // Check that the event exists and belongs to this Organiser.
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        if (existingEvent == null ||
            existingEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Events));
        }

        // Transfer the existing event details into the form.
        var model = new EventFormViewModel
        {
            EventID = existingEvent.EventID,
            EventName = existingEvent.EventName,
            EventDate = existingEvent.EventDate,
            Location = existingEvent.Location,
            Description = existingEvent.Description,
            Status = existingEvent.Status
        };

        return View(model);
    }
    // Submit updated event details to the API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(
        int id, EventFormViewModel model)
    {
        if (id != model.EventID)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.EventDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                "EventDate",
                "Event date cannot be in the past.");

            return View(model);
        }

        var existingEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{id}");

        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        if (existingEvent == null ||
            existingEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Events));
        }

        var updatedEvent = new
        {
            EventName = model.EventName,
            EventDate = model.EventDate,
            Location = model.Location,
            Description = model.Description,
            Status = model.Status
        };

        var response = await _apiService.PutAsync<object, EventViewModel>(
            $"api/Events/{id}", updatedEvent);

        if (response == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to update the event. Please try again.");

            return View(model);
        }

        TempData["SuccessMessage"] = "Event updated successfully!";

        return RedirectToAction(nameof(Events));
    }
    // Delete an existing event through the API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        // Retrieve the event to verify ownership.
        var existingEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{id}");

        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        if (existingEvent == null ||
            existingEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Events));
        }

        // Request deletion through the API.
        bool deleted = await _apiService.DeleteAsync($"api/Events/{id}");

        if (!deleted)
        {
            TempData["ErrorMessage"] =
                "The event could not be deleted. It may have existing categories, routes or weather records.";

            return RedirectToAction(nameof(Events));
        }

        TempData["SuccessMessage"] = "Event deleted successfully!";

        return RedirectToAction(nameof(Events));
    }
    // Display categories belonging to an Organiser's event.
    [HttpGet]
    public async Task<IActionResult> Categories(int eventId)
    {
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        // Retrieve the selected event from the API.
        var raceEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{eventId}");

        if (raceEvent == null || raceEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Events));
        }

        // Retrieve categories belonging to this event.
        var categories = await _apiService.GetAsync<List<CategoryViewModel>>(
            $"api/Categories/event/{eventId}") ?? new List<CategoryViewModel>();

        var model = new OrganiserCategoriesViewModel
        {
            Event = raceEvent,
            Categories = categories
        };

        return View(model);
    }
    // Display the form for creating a category.
    [HttpGet]
    public async Task<IActionResult> CreateCategory(int eventId)
    {
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        // Retrieve the event from the API.
        var raceEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{eventId}");

        // Confirm that the event belongs to this Organiser.
        if (raceEvent == null || raceEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Events));
        }

        // Categories can only be added to open events.
        if (raceEvent.Status != "Open")
        {
            TempData["ErrorMessage"] =
                "Categories can only be added to open events.";

            return RedirectToAction(nameof(Categories),
                new { eventId });
        }

        var model = new CategoryFormViewModel
        {
            EventID = eventId
        };

        ViewBag.EventName = raceEvent.EventName;

        return View(model);
    }
    // Submit a new category to the API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(
        CategoryFormViewModel model)
    {
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        // Verify that the selected event belongs to this Organiser.
        var raceEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{model.EventID}");

        if (raceEvent == null || raceEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToAction(nameof(Events));
        }

        if (raceEvent.Status != "Open")
        {
            TempData["ErrorMessage"] =
                "Categories can only be added to open events.";

            return RedirectToAction(nameof(Categories),
                new { eventId = model.EventID });
        }

        ViewBag.EventName = raceEvent.EventName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Send the category details to the API.
        var request = new
        {
            EventID = model.EventID,
            CategoryName = model.CategoryName,
            DistanceKM = model.DistanceKM,
            EntryFee = model.EntryFee,
            MaxParticipants = model.MaxParticipants
        };

        var response = await _apiService.PostAsync<object, CategoryViewModel>(
            "api/Categories", request);

        if (response == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to create the category. Please check the details and try again.");

            return View(model);
        }

        TempData["SuccessMessage"] = "Category created successfully!";

        return RedirectToAction(nameof(Categories),
            new { eventId = model.EventID });
    }
    // Display the form for editing an existing category.
    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        // Retrieve the category from the API.
        var category = await _apiService.GetAsync<CategoryViewModel>(
            $"api/Categories/{id}");

        if (category == null)
        {
            TempData["ErrorMessage"] = "Category not found.";
            return RedirectToAction(nameof(Events));
        }

        // Retrieve the event associated with the category.
        var raceEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{category.EventID}");

        if (raceEvent == null || raceEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "You cannot edit this category.";
            return RedirectToAction(nameof(Events));
        }

        if (raceEvent.Status != "Open")
        {
            TempData["ErrorMessage"] =
                "Categories can only be updated for open events.";

            return RedirectToAction(nameof(Categories),
                new { eventId = category.EventID });
        }

        var model = new CategoryFormViewModel
        {
            CategoryID = category.CategoryID,
            EventID = category.EventID,
            CategoryName = category.CategoryName,
            DistanceKM = category.DistanceKM,
            EntryFee = category.EntryFee,
            MaxParticipants = category.MaxParticipants
        };

        ViewBag.EventName = raceEvent.EventName;

        return View(model);
    }
    // Submit updated category details to the API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(
        int id, CategoryFormViewModel model)
    {
        if (id != model.CategoryID)
        {
            return BadRequest();
        }

        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        var existingCategory = await _apiService.GetAsync<CategoryViewModel>(
            $"api/Categories/{id}");

        if (existingCategory == null)
        {
            TempData["ErrorMessage"] = "Category not found.";
            return RedirectToAction(nameof(Events));
        }

        var raceEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{existingCategory.EventID}");

        if (raceEvent == null || raceEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "You cannot edit this category.";
            return RedirectToAction(nameof(Events));
        }

        if (raceEvent.Status != "Open")
        {
            TempData["ErrorMessage"] =
                "Categories can only be updated for open events.";

            return RedirectToAction(nameof(Categories),
                new { eventId = existingCategory.EventID });
        }

        model.EventID = existingCategory.EventID;
        ViewBag.EventName = raceEvent.EventName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new
        {
            CategoryName = model.CategoryName,
            DistanceKM = model.DistanceKM,
            EntryFee = model.EntryFee,
            MaxParticipants = model.MaxParticipants
        };

        var response = await _apiService.PutAsync<object, CategoryViewModel>(
            $"api/Categories/{id}", request);

        if (response == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to update the category. Please try again.");

            return View(model);
        }

        TempData["SuccessMessage"] = "Category updated successfully!";

        return RedirectToAction(nameof(Categories),
            new { eventId = model.EventID });
    }
    // Delete a category through the API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        int organiserId = HttpContext.Session.GetInt32("UserID") ?? 0;

        // Retrieve the category to verify ownership.
        var category = await _apiService.GetAsync<CategoryViewModel>(
            $"api/Categories/{id}");

        if (category == null)
        {
            TempData["ErrorMessage"] = "Category not found.";
            return RedirectToAction(nameof(Events));
        }

        // Retrieve the event associated with the category.
        var raceEvent = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{category.EventID}");

        if (raceEvent == null || raceEvent.OrganiserID != organiserId)
        {
            TempData["ErrorMessage"] = "You cannot delete this category.";
            return RedirectToAction(nameof(Events));
        }

        // Send the delete request to the API.
        bool deleted = await _apiService.DeleteAsync(
            $"api/Categories/{id}");

        if (!deleted)
        {
            TempData["ErrorMessage"] =
                "Category could not be deleted. It may have existing enrolments.";

            return RedirectToAction(nameof(Categories),
                new { eventId = category.EventID });
        }

        TempData["SuccessMessage"] = "Category deleted successfully!";

        return RedirectToAction(nameof(Categories),
            new { eventId = category.EventID });
    }
    // Display enrolments for events belonging to the logged-in Organiser.
    public async Task<IActionResult> Enrolments()
    {
        var enrolments =
            await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                "api/Enrolments/organiser");

        if (enrolments == null)
        {
            ViewBag.ErrorMessage =
                "Unable to load enrolments. Please try again.";

            enrolments = new List<OrganiserEnrolmentViewModel>();
        }

        return View(enrolments);
    }
    // Display results and available enrolments for the Organiser.
    public async Task<IActionResult> Results()
    {
        var results = new List<OrganiserResultViewModel>();

        var apiResults =
            await _apiService.GetAsync<List<ResultViewModel>>(
                "api/Results");

        var enrolments =
            await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                "api/Enrolments/organiser");

        // Match results with their participant, event and category details.
        if (apiResults != null && enrolments != null)
        {
            foreach (var result in apiResults)
            {
                var enrolment = enrolments.FirstOrDefault(
                    e => e.EnrolmentID == result.EnrolmentID);

                if (enrolment != null)
                {
                    results.Add(new OrganiserResultViewModel
                    {
                        ResultID = result.ResultID,
                        EnrolmentID = result.EnrolmentID,
                        ParticipantName = enrolment.ParticipantName,
                        EventName = enrolment.EventName,
                        CategoryName = enrolment.CategoryName,
                        FinishTime = result.FinishTime,
                        OverallPosition = result.OverallPosition,
                        CategoryPosition = result.CategoryPosition,
                        ResultStatus = result.ResultStatus
                    });
                }
            }
        }

        var pageModel = new OrganiserResultsPageViewModel
        {
            Results = results,
            Enrolments = enrolments ?? new List<OrganiserEnrolmentViewModel>()
        };

        return View(pageModel);
    }
    // Display the form for recording a race result.
    [HttpGet]
    public async Task<IActionResult> CreateResult()
    {
        var enrolments =
            await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                "api/Enrolments/organiser");

        if (enrolments == null || !enrolments.Any())
        {
            TempData["ErrorMessage"] =
                "No enrolments are available for recording results.";

            return RedirectToAction(nameof(Results));
        }

        ViewBag.Enrolments = enrolments;

        return View(new ResultFormViewModel());
    }


    // Submit a new race result to the API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResult(ResultFormViewModel model)
    {
        var enrolments =
            await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                "api/Enrolments/organiser");

        ViewBag.Enrolments =
            enrolments ?? new List<OrganiserEnrolmentViewModel>();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new
        {
            model.EnrolmentID,
            model.FinishTime,
            model.OverallPosition,
            model.CategoryPosition,
            model.ResultStatus
        };

        var response =
            await _apiService.PostForStringAsync(
                "api/Results", request);

        if (response == null || response.StartsWith("API ERROR"))
        {
            ModelState.AddModelError(string.Empty,
                response ?? "Unable to record the result.");

            return View(model);
        }

        TempData["SuccessMessage"] = "Race result recorded successfully!";

        return RedirectToAction(nameof(Results));
    }
    // Display the form for editing an existing result.
    [HttpGet]
    public async Task<IActionResult> EditResult(int id)
    {
        var result = await _apiService.GetAsync<ResultViewModel>(
            $"api/Results/{id}");

        if (result == null)
        {
            TempData["ErrorMessage"] = "Result not found.";
            return RedirectToAction(nameof(Results));
        }

        var enrolments =
            await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                "api/Enrolments/organiser");

        var enrolment = enrolments?.FirstOrDefault(
            e => e.EnrolmentID == result.EnrolmentID);

        if (enrolment == null)
        {
            TempData["ErrorMessage"] =
                "You cannot edit this result.";

            return RedirectToAction(nameof(Results));
        }

        ViewBag.EnrolmentDetails = enrolment;

        var model = new ResultFormViewModel
        {
            ResultID = result.ResultID,
            EnrolmentID = result.EnrolmentID,
            FinishTime = result.FinishTime,
            OverallPosition = result.OverallPosition,
            CategoryPosition = result.CategoryPosition,
            ResultStatus = result.ResultStatus
        };

        return View(model);
    }


    // Submit changes to an existing result.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResult(
        int id, ResultFormViewModel model)
    {
        var existingResult =
            await _apiService.GetAsync<ResultViewModel>(
                $"api/Results/{id}");

        if (existingResult == null ||
            existingResult.EnrolmentID != model.EnrolmentID)
        {
            TempData["ErrorMessage"] = "Result not found or invalid.";
            return RedirectToAction(nameof(Results));
        }

        var enrolments =
            await _apiService.GetAsync<List<OrganiserEnrolmentViewModel>>(
                "api/Enrolments/organiser");

        var enrolment = enrolments?.FirstOrDefault(
            e => e.EnrolmentID == existingResult.EnrolmentID);

        if (enrolment == null)
        {
            TempData["ErrorMessage"] =
                "You cannot edit this result.";

            return RedirectToAction(nameof(Results));
        }

        ViewBag.EnrolmentDetails = enrolment;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new
        {
            model.FinishTime,
            model.OverallPosition,
            model.CategoryPosition,
            model.ResultStatus
        };

        var updatedResult =
            await _apiService.PutAsync<object, ResultViewModel>(
                $"api/Results/{id}", request);

        if (updatedResult == null)
        {
            ModelState.AddModelError(string.Empty,
                "Unable to update the result. Please check the information and try again.");

            return View(model);
        }

        TempData["SuccessMessage"] = "Result updated successfully!";

        return RedirectToAction(nameof(Results));
    }
}
