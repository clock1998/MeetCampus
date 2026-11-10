namespace MeetCampus.Features.Matching.Services;

public static class IntentCompatibilityService
{
    /// <summary>Checks whether both members have a compatible intent in each direction.</summary>
    public static bool AreCompatible(IReadOnlyCollection<string> firstIntents, IReadOnlyCollection<string> secondIntents)
    {
        ArgumentNullException.ThrowIfNull(firstIntents);
        ArgumentNullException.ThrowIfNull(secondIntents);

        foreach (var intent in firstIntents.Concat(secondIntents))
        {
            if (intent is not ("friendship" or "dating" or "study-partner" or "activity-partner"))
                throw new InvalidOperationException("A selected intent does not have a compatibility rule.");
        }

        return HasAllowedPair(firstIntents, secondIntents) && HasAllowedPair(secondIntents, firstIntents);
    }

    private static bool HasAllowedPair(IEnumerable<string> senderIntents, IReadOnlyCollection<string> recipientIntents) =>
        senderIntents.Any(sender => recipientIntents.Any(recipient => IsAllowed(sender, recipient)));

    private static bool IsAllowed(string sender, string recipient) => (sender, recipient) switch
    {
        ("friendship", "friendship") or ("dating", "dating")
            or ("study-partner", "study-partner") or ("activity-partner", "activity-partner") => true,
        _ => false
    };
}
