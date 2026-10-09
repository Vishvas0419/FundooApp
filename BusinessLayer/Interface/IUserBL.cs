using Models;

namespace BusinessLayer.Interface;

public interface IUserBL
{
    //contract
    public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register);
    public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login);
}