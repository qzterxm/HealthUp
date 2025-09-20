using DataAccess.Enums;
using DataAccess.Models;
using Refit;

namespace UI.ApiClients;

public interface IAuthApiClient
{
    [Post("/Auth/register")]
    Task<RegistrationResponse> Register(RegistrationUser user);
    
    [Post("/Auth/login")]
    Task<TokenDTO> Login(LoginUser request);
    
    [Post("/Auth/refresh")]
    Task<TokenDTO> Refresh(TokenDTO token);
}
public class RegistrationResponse
{
    public string Message { get; set; }
    public string ActivationLink { get; set; }
}
