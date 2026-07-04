namespace AgainDj.Domain.Model;

/// <summary>A planned blend from one deck to another.</summary>
public sealed record TransitionPlan
{
    public required DeckId FromDeck { get; init; }

    public required DeckId ToDeck { get; init; }

    public required TransitionTechnique Technique { get; init; }

    public required double DurationSeconds { get; init; }

    public string? Reason { get; init; }
}
