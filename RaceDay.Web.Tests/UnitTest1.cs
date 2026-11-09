
using System.ComponentModel.DataAnnotations;
using RaceDay.Web.Models;
using Xunit;

namespace RaceDay.Web.Tests;

public class ProfileValidationTests
{
    // Test that a profile with missing required fields is invalid.
    [Fact]
    public void Profile_WithMissingRequiredFields_IsInvalid()
    {
        var profile = new ProfileViewModel();

        var results = ValidateModel(profile);

        Assert.NotEmpty(results);
    }

    // Test that an invalid email address is rejected.
    [Fact]
    public void Profile_WithInvalidEmail_IsInvalid()
    {
        var profile = new ProfileViewModel
        {
            FullName = "Test Participant",
            Email = "invalid-email"
        };

        var results = ValidateModel(profile);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(ProfileViewModel.Email)));
    }

    // Test that a correctly completed profile is valid.
    [Fact]
    public void Profile_WithValidDetails_IsValid()
    {
        var profile = new ProfileViewModel
        {
            FullName = "Test Participant",
            Email = "participant@example.com",
            Phone = "0821234567"
        };

        var results = ValidateModel(profile);

        Assert.Empty(results);
    }

    // Helper method to execute the model's validation attributes.
    private static List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();

        var context = new ValidationContext(model);

        Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        return results;
    }
}
