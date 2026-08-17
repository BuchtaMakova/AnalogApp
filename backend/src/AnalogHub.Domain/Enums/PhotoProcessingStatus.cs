namespace AnalogHub.Domain.Enums;

/// <summary>Tracks the async pipeline: presigned upload -> registration -> background derivative generation.</summary>
public enum PhotoProcessingStatus
{
    PendingUpload = 1,
    Uploaded = 2,
    Processing = 3,
    Ready = 4,
    Failed = 5
}
