using DataAccess.Models;
using Refit;

namespace UI.ApiClients;

public interface IUserApiClient
{
    [Get("/User/GetAllUsers")]
    Task<List<UserDTO>> GetAllUsers();
    
    [Get("/User/GetUserById/{id}")]
    Task<UserDTO> GetUserById(Guid id);
    
    
    [Get("/User/Activate/{id}")]
    Task<string> Activate(Guid id);
    
    [Put("/User/UpdateUser/")]
    Task<bool> Update(UserDTO userDto);
    
    [Delete("/User/DeleteUser/{id}")]
    Task<string> Delete(Guid id);

    [Put("/User/ChangePassword/{id}")]
    Task ChangePassword(Guid id, string oldPassword, string newPassword);
}