using Models;

namespace RepositoryLayer.Interface;

public interface IUserRL
{
    public ResponseModel<RegistrationModel> RegisterUserRL(RegistrationModel register);
    public ResponseModel<LoginModel> LoginUserRL(LoginModel login);
}