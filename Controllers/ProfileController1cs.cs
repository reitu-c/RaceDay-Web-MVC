using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorization("Participant")]
public class ProfileController : Controller
{
    private readonly RaceDayApiService _apiService;

    public ProfileController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    // Display the participant's profile.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profile = await _apiService.GetAsync<ProfileViewModel>(
            "api/Users/profile");

        if (profile == null)
        {
            TempData["ErrorMessage"] =
                "Unable to retrieve your profile. Please try again.";

            return RedirectToAction("Index", "Participant");
        }

        return View(profile);
    }

    // Update the participant's personal details.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var request = new
        {
            model.FullName,
            model.Email,
            model.Phone
        };

        var response = await _apiService.PutAsync<object, System.Text.Json.JsonElement>(
            "api/Users/profile", request);

        if (response.ValueKind == System.Text.Json.JsonValueKind.Undefined)
        {
            TempData["ErrorMessage"] =
                "Unable to update your profile. Please check your details.";

            return View(model);
        }

        HttpContext.Session.SetString("FullName", model.FullName);
        HttpContext.Session.SetString("Email", model.Email);

        TempData["SuccessMessage"] =
            "Your profile details have been updated successfully.";

        return RedirectToAction("Index");
    }

    // Upload a new profile picture through the REST API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] =
                "Please select a profile picture.";

            return RedirectToAction("Index");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            TempData["ErrorMessage"] =
                "The profile picture cannot exceed 5 MB.";

            return RedirectToAction("Index");
        }

        var response = await _apiService.PostFileAsync(
            "api/Users/profile-picture", file);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                $"Profile picture upload failed: {error}";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Your profile picture has been uploaded successfully.";
        }

        response.Dispose();

        return RedirectToAction("Index");
    }

    // Display the private profile picture retrieved through the API.
    [HttpGet]
    public async Task<IActionResult> Picture()
    {
        var image = await _apiService.GetFileAsync(
            "api/Users/profile-picture");

        if (image == null)
            return NotFound();

        return File(image.Value.Data, image.Value.ContentType);
    }
}
