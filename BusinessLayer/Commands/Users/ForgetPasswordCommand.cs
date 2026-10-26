using MediatR;
using Models;

namespace BusinessLayer.Commands.Users;

public class ForgetPasswordCommand : IRequest<ResponseModel<string>>
{
    public ForgetPasswordModel ForgetPasswordModel { get; set; } = new();
}