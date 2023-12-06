namespace Domain.Interfaces;

public interface IJwtTokenProvider
{
    Task<string> GetToken(CancellationToken cancellationToken = default(CancellationToken));
}