using Domain.Entity;
using Domain.Model.View;

namespace Domain.Model.Response;

public class QueryCommentsResponse : Response
{
    public IEnumerable<CommentModel> Comments { get; set; }
    public IEnumerable<UserModel> Users { get; set; }
}