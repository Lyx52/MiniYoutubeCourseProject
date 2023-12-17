using Domain.Model.View;

namespace Domain.Model.Response;

public class CreatorProfileResponse : Response
{
    public UserModel Creator { get; set; }
    public bool IsSubscribed { get; set; }
    public long SubscriberCount { get; set; }
}