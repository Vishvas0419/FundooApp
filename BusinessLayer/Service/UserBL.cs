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
    
    public RegistrationModel RegisterUserBL( RegistrationModel registrationModel)
    {
        return userRL.RegisterUserRL(registrationModel);
    }
}