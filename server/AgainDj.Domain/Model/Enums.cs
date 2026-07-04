namespace AgainDj.Domain.Model;

/// <summary>Physical deck identity on the console.</summary>
public enum DeckId
{
    A,
    B,
}

/// <summary>EQ band on a channel strip.</summary>
public enum EqBand
{
    Low,
    Mid,
    High,
}

/// <summary>Who performed an action — lets the UI show the AI "pressing" controls.</summary>
public enum Actor
{
    Human,
    Ai,
}

/// <summary>Who is currently driving the console.</summary>
public enum SessionMode
{
    Human,
    Autonomous,
}

/// <summary>The kind of control an action manipulates.</summary>
public enum DjActionType
{
    LoadTrack,
    Play,
    Pause,
    Cue,
    SetTempo,
    Nudge,
    SetVolume,
    SetEq,
    SetCrossfader,
    TriggerSample,
    StartTransition,
    Sync,
}

/// <summary>A blend technique used when moving from one track to the next.</summary>
public enum TransitionTechnique
{
    Cut,
    Crossfade,
    BasslineSwap,
    EchoOut,
    FilterFade,
}
