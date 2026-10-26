using MediatR;
using Models;

namespace BusinessLayer.Commands.Users;

public class RegisterUserCommand : IRequest<ResponseModel<string>>
{
    public RegistrationModel RegisterModel { get; set; } = new RegistrationModel();
}