using Models;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface;

public interface IUserRL
{
    public ResponseModel<RegistrationModel> RegisterUserRL(RegistrationModel register);
    public UserEntity? LoginUserRL(LoginModel login);
    public UserEntity? FindUserByEmail(string email);
    public UserEntity? GetUserById(int userId);
    public void UpdatePassword(UserEntity user);
}