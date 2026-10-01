using BusinessLayer.Interface;
using Models;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service;

public class UserBL : IUserBL
{
    public IUserRL userRL;

    public UserBL(IUserRL userRL)
    {
        this.userRL = userRL;
    }
    
    public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register)
    {
        return userRL.RegisterUserRL(register);
    }

    public ResponseModel<LoginModel> LoginUserBL(LoginModel login)
    {
        return userRL.LoginUserRL(login);
    }
}