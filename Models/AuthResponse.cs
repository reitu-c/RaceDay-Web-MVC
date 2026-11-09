namespace RaceDay.Web.Models;

public class AuthResponse
{
    public string? Message { get; set; }

    public int UserID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}

