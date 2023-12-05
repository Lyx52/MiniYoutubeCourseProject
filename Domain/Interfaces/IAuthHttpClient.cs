using Domain.Model;
using Domain.Model.Response;
using Domain.Model.View;

namespace Domain.Interfaces;

public interface IAuthHttpClient
{
    Task<LoginResponse> LoginAsync(LoginModel model);
    Task<Response> RegisterAsync(RegisterModel model);
}