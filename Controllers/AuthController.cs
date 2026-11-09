using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AuthController : Controller
{
    private readonly RaceDayApiService _apiService;

    public AuthController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    // GET: Auth/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: Auth/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Clear any previous MVC session before starting a new login
        HttpContext.Session.Clear();

        // Public registration is restricted to Participants
        var request = new
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password,
            Role = "Participant",
            Phone = model.Phone
        };

        var response = await _apiService.PostAsync<object, AuthResponse>(
            "api/Auth/register", request);

        if (response == null || response.UserID <= 0)
        {
            ModelState.AddModelError(
                "",
                "Registration could not be completed. " +
                "The email may already be registered or the API is unavailable.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Registration successful! You can now log in.";

        return RedirectToAction("Login");
    }

    // GET: Auth/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: Auth/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Send login details to the Part 2 API.
        var request = new
        {
            Email = model.Email,
            Password = model.Password
        };

        var response = await _apiService.PostAsync<object, AuthResponse>(
            "api/Auth/login", request);

        if (response == null ||
            response.UserID <= 0 ||
            string.IsNullOrWhiteSpace(response.Role))
        {
            ModelState.AddModelError(
                "",
                "Invalid email or password. Please try again.");

            return View(model);
        }

        // Only allow the two supported roles.
        if (response.Role != "Organiser" &&
            response.Role != "Participant")
        {
            ModelState.AddModelError(
                "",
                "Your account has an unsupported role.");

            return View(model);
        }

        // Store authenticated user information.
        HttpContext.Session.SetInt32(
            "UserID", response.UserID);

        HttpContext.Session.SetString(
            "FullName", response.FullName);

        HttpContext.Session.SetString(
            "Email", response.Email);

        HttpContext.Session.SetString(
            "Role", response.Role);

        // Redirect according to the user's role.
        if (response.Role == "Organiser")
        {
            return RedirectToAction(
                "Dashboard", "Organiser");
        }

        return RedirectToAction(
            "Index", "Participant");
    }

    // POST: Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // End the session maintained by the Part 2 API.
        await _apiService.PostAsync<object, AuthResponse>(
            "api/Auth/logout", new { });

        // Clear the MVC session.
        HttpContext.Session.Clear();

        TempData["SuccessMessage"] =
            "You have successfully logged out.";

        return RedirectToAction("Login");
    }
}

