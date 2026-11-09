using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorization("Participant")]
public class ParticipantController : Controller
{
    private readonly RaceDayApiService _apiService;

    // Inject the API service into the controller.
    public ParticipantController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    // Participant home page.
    public IActionResult Index()
    {
        ViewBag.FullName = HttpContext.Session.GetString("FullName");
        return View();
    }

    // Display events retrieved from the RaceDay REST API.
    public async Task<IActionResult> Events()
    {
        var events = await _apiService.GetAsync<List<EventViewModel>>(
            "api/Events");

        if (events == null)
        {
            ViewBag.ErrorMessage =
                "Unable to retrieve events. Please check your connection and try again.";

            return View(new List<EventViewModel>());
        }

        return View(events);
    }

    // Display the full details of one selected event.
    public async Task<IActionResult> EventDetails(int id)
    {
        var eventItem = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{id}");

        if (eventItem == null)
        {
            TempData["ErrorMessage"] = "The selected event could not be found.";
            return RedirectToAction("Events");
        }

        return View(eventItem);
    }

    // Display the categories available for a selected event.
    public async Task<IActionResult> EventCategories(int eventId)
    {
        // Retrieve the selected event from the REST API.
        var eventItem = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{eventId}");

        if (eventItem == null)
        {
            TempData["ErrorMessage"] = "The selected event could not be found.";
            return RedirectToAction("Events");
        }

        // Retrieve the categories belonging to this event.
        var categories = await _apiService.GetAsync<List<CategoryViewModel>>(
            $"api/Categories/event/{eventId}");

        // Combine the event and its categories.
        var viewModel = new EventCategoriesViewModel
        {
            Event = eventItem,
            Categories = categories ?? new List<CategoryViewModel>()
        };

        return View(viewModel);
    }

    // Display the enrolment form for a selected event.
    [HttpGet]
    public async Task<IActionResult> Enrol(int eventId)
    {
        // Retrieve the selected event.
        var eventItem = await _apiService.GetAsync<EventViewModel>(
            $"api/Events/{eventId}");

        if (eventItem == null)
        {
            TempData["ErrorMessage"] = "The selected event could not be found.";
            return RedirectToAction("Events");
        }

        // Retrieve the event's available categories.
        var categories = await _apiService.GetAsync<List<CategoryViewModel>>(
            $"api/Categories/event/{eventId}");

        var viewModel = new EnrolmentPageViewModel
        {
            Event = eventItem,
            Categories = categories ?? new List<CategoryViewModel>()
        };

        return View(viewModel);
    }

    // Submit a Participant's event enrolment to the REST API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(
        int eventId,
        EnrolmentViewModel model)
    {
        if (!ModelState.IsValid || model.CategoryID <= 0)
        {
            TempData["ErrorMessage"] =
                "Please select a valid race category.";

            return RedirectToAction("Enrol", new { eventId });
        }

        // Send only the fields required by the API.
        var request = new
        {
            CategoryID = model.CategoryID,
            EmergencyContact = model.EmergencyContact
        };

        var response = await _apiService.PostForStringAsync(
            "api/Enrolments", request);


        if (response == null || response.StartsWith("API ERROR"))
        {
            TempData["ErrorMessage"] = response ??
                "The API did not return a response.";

            return RedirectToAction("Enrol", new { eventId });
        }


        TempData["SuccessMessage"] =
            "You have successfully enrolled in this event!";

        return RedirectToAction("EventDetails", new { id = eventId });
    }

    // Display all events in which the current Participant has enrolled.
    public async Task<IActionResult> MyEvents()
    {
        // Retrieve the Participant's enrolments.
        var enrolments = await _apiService.GetAsync<List<EnrolmentRecordViewModel>>(
            "api/Enrolments");

        if (enrolments == null)
        {
            ViewBag.ErrorMessage =
                "Unable to retrieve your enrolments. Please try again.";

            return View(new List<MyEventViewModel>());
        }

        var myEvents = new List<MyEventViewModel>();

        // Retrieve the event and category details for each enrolment.
        foreach (var enrolment in enrolments)
        {
            var category = await _apiService.GetAsync<CategoryViewModel>(
                $"api/Categories/{enrolment.CategoryID}");

            if (category == null)
                continue;

            var eventItem = await _apiService.GetAsync<EventViewModel>(
                $"api/Events/{category.EventID}");

            if (eventItem == null)
                continue;

            myEvents.Add(new MyEventViewModel
            {
                Enrolment = enrolment,
                Event = eventItem,
                Category = category
            });
        }

        return View(myEvents);
    }

    // Cancel a Participant's enrolment through the REST API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        bool success = await _apiService.DeleteAsync(
            $"api/Enrolments/{id}");

        if (success)
        {
            TempData["SuccessMessage"] =
                "Your enrolment has been cancelled successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                "Unable to cancel your enrolment. Please try again.";
        }

        return RedirectToAction("MyEvents");
    }
    // Display results belonging to the logged-in Participant.
    public async Task<IActionResult> MyResults()
    {
        // Retrieve the Participant's enrolments.
        var enrolments = await _apiService.GetAsync<List<EnrolmentRecordViewModel>>(
            "api/Enrolments");

        if (enrolments == null)
        {
            ViewBag.ErrorMessage =
                "Unable to retrieve your results. Please try again.";

            return View(new List<MyResultViewModel>());
        }

        var myResults = new List<MyResultViewModel>();

        // Check each enrolment for an available race result.
        foreach (var enrolment in enrolments)
        {
            var result = await _apiService.GetAsync<ResultViewModel>(
                $"api/Results/enrolment/{enrolment.EnrolmentID}");

            // Skip enrolments that do not have results yet.
            if (result == null)
                continue;

            var category = await _apiService.GetAsync<CategoryViewModel>(
                $"api/Categories/{enrolment.CategoryID}");

            if (category == null)
                continue;

            var eventItem = await _apiService.GetAsync<EventViewModel>(
                $"api/Events/{category.EventID}");

            if (eventItem == null)
                continue;

            myResults.Add(new MyResultViewModel
            {
                Result = result,
                Enrolment = enrolment,
                Event = eventItem,
                Category = category
            });
        }

        return View(myResults);
    }
}


