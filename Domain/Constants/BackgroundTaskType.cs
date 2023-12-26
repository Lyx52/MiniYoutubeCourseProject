namespace Domain.Constants;

public enum BackgroundTaskType
{
    ProcessVideo,
    PublishVideo,
    DeleteVideo,
    IncrementVideoViewCount,
    GenerateUploadNotifications,
    SendConfirmationEmail
}