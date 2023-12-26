namespace Domain.Constants;

public enum VideoProcessingStatus
{
    CreatedMetadata,
    Processing,
    ProcessingFinished,
    ProcessingFailed,
    Published,
    Deleting
}