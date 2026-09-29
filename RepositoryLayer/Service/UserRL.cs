using Models;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Service;

public class UserRL : IUserRL
{
    FundooContext context;

    public UserRL(FundooContext context)
    {
        this.context = context;
    }
    
    
    public RegistrationModel RegisterUserRL(RegistrationModel register)
    {
        UserEntity user = new UserEntity();
        user.FirstName = register.FirstName;
        user.LastName = register.LastName;
        user.Email = register.Email;
        user.Password = register.Password;
        user.PhoneNumber = register.ContactNumber;
        context.Users.Add(user);
      var result =   context.SaveChanges();
      Console.WriteLine(result);
        return register;
    }
    
}