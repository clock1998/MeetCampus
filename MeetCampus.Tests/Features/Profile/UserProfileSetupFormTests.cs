using System.ComponentModel.DataAnnotations;
using MeetCampus.Contracts.Features.Profile;
using Xunit;

namespace MeetCampus.Tests.Features.Profile;

public sealed class UserProfileSetupFormTests
{
    [Fact]
    public void WhenRequiredFieldsAreMissingThenValidationFails()
    {
        var form = new UserProfileSetupForm();
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            form,
            new ValidationContext(form),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Equal(
            new[]
            {
                nameof(UserProfileSetupForm.EthnicityId),
                nameof(UserProfileSetupForm.IntentionIds),
                nameof(UserProfileSetupForm.LanguageId),
                nameof(UserProfileSetupForm.SchoolId),
                nameof(UserProfileSetupForm.StudyDomainId),
            },
            validationResults.SelectMany(result => result.MemberNames).OrderBy(name => name));
    }

    [Fact]
    public void WhenRequiredFieldsAreProvidedThenValidationSucceeds()
    {
        var form = new UserProfileSetupForm
        {
            StudyDomainId = Guid.NewGuid(),
            SchoolId = Guid.NewGuid(),
            LanguageId = Guid.NewGuid(),
            IntentionIds = [Guid.NewGuid()],
            EthnicityId = Guid.NewGuid(),
        };

        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            form,
            new ValidationContext(form),
            validationResults,
            validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    public void WhenIntentCountIsValidatedThenOnlyOneOrTwoAreAllowed(int count, bool expected)
    {
        var form = new UserProfileSetupForm
        {
            StudyDomainId = Guid.NewGuid(), SchoolId = Guid.NewGuid(), LanguageId = Guid.NewGuid(),
            IntentionIds = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList(),
            EthnicityId = Guid.NewGuid()
        };

        var result = Validator.TryValidateObject(form, new ValidationContext(form), [], validateAllProperties: true);

        Assert.Equal(expected, result);
    }
}
