using MediatR;
using Models;

namespace BusinessLayer.Queries.Users;

public class LoginUserQuery : IRequest<ResponseModel<LoginResponseModel>>
{
    public LoginModel LoginModel { get; set; } = new();
}