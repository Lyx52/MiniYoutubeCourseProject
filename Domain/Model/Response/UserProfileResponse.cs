using Domain.Model.View;

namespace Domain.Model.Response;

public class UserProfileResponse : Response
{
    public UserModel User { get; set; }
}