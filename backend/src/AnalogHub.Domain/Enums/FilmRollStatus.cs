namespace AnalogHub.Domain.Enums;

/// <summary>Lifecycle of a physical roll of film, from loading in a camera to archival after scanning.</summary>
public enum FilmRollStatus
{
    Loaded = 1,
    InProgress = 2,
    ShotCompleted = 3,
    SentToLab = 4,
    Developed = 5,
    Scanned = 6,
    Archived = 7
}
