using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters;

public class SessionAuthorizationAttribute : ActionFilterAttribute
{
    private readonly string? _requiredRole;

    public SessionAuthorizationAttribute(string? requiredRole = null)
    {
        _requiredRole = requiredRole;
    }

    public override void OnActionExecuting(
        ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        int? userId = session.GetInt32("UserID");
        string? role = session.GetString("Role");

        // Redirect users who are not logged in.
        if (userId == null || string.IsNullOrEmpty(role))
        {
            context.Result = new RedirectToActionResult(
                "Login", "Auth", null);

            return;
        }

        // Check whether the user has the required role.
        if (!string.IsNullOrEmpty(_requiredRole) &&
            !string.Equals(
                role,
                _requiredRole,
                StringComparison.OrdinalIgnoreCase))
        {
            // Redirect the user to their own dashboard.
            if (role == "Organiser")
            {
                context.Result = new RedirectToActionResult(
                    "Dashboard", "Organiser", null);
            }
            else if (role == "Participant")
            {
                context.Result = new RedirectToActionResult(
                    "Index", "Participant", null);
            }
            else
            {
                context.HttpContext.Session.Clear();

                context.Result = new RedirectToActionResult(
                    "Login", "Auth", null);
            }
        }
    }
}

