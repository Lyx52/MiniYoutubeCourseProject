using Domain.Entity;
using Domain.Model.View;

namespace WebApi.Services.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserModel>> GetUsersByIds(IEnumerable<string> userIds,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<UserModel?> GetUserById(string userId, CancellationToken cancellationToken = default(CancellationToken));
}