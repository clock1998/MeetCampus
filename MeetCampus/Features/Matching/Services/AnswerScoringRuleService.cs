namespace MeetCampus.Features.Matching.Services;

public static class AnswerScoringRuleService
{
    /// <summary>Compares two selected option IDs on a scale from 0 to 100.</summary>
    public static decimal ScoreSingleChoice(Guid firstOptionId, Guid secondOptionId)
    {
        if (firstOptionId == Guid.Empty)
            throw new ArgumentException("An option ID is required.", nameof(firstOptionId));
        if (secondOptionId == Guid.Empty)
            throw new ArgumentException("An option ID is required.", nameof(secondOptionId));

        return firstOptionId == secondOptionId ? 100m : 0m;
    }

    /// <summary>Scores the overlap between two nonempty sets of option IDs from 0 to 100.</summary>
    public static decimal ScoreMultipleChoice(
        IReadOnlyCollection<Guid> firstOptionIds,
        IReadOnlyCollection<Guid> secondOptionIds)
    {
        ArgumentNullException.ThrowIfNull(firstOptionIds);
        ArgumentNullException.ThrowIfNull(secondOptionIds);

        var first = ValidateOptions(firstOptionIds, nameof(firstOptionIds));
        var second = ValidateOptions(secondOptionIds, nameof(secondOptionIds));
        var sharedCount = first.Count(second.Contains);

        return 200m * sharedCount / ((decimal)first.Count + second.Count);
    }

    /// <summary>Scores two scale answers by their distance within the specified range.</summary>
    public static decimal ScoreScale(int firstValue, int secondValue, int minimum, int maximum)
    {
        if (minimum >= maximum)
            throw new ArgumentException("The scale minimum must be less than the maximum.", nameof(minimum));
        if (firstValue < minimum || firstValue > maximum)
            throw new ArgumentOutOfRangeException(nameof(firstValue));
        if (secondValue < minimum || secondValue > maximum)
            throw new ArgumentOutOfRangeException(nameof(secondValue));

        var distance = Math.Abs((decimal)firstValue - secondValue);
        var range = (decimal)maximum - minimum;
        return 100m * (1m - distance / range);
    }

    private static HashSet<Guid> ValidateOptions(IReadOnlyCollection<Guid> optionIds, string parameterName)
    {
        var options = optionIds.ToHashSet();
        if (options.Count == 0 || options.Count != optionIds.Count || options.Contains(Guid.Empty))
            throw new ArgumentException("Options must contain distinct, nonempty IDs.", parameterName);

        return options;
    }
}
