using MediatR;
using Models;

namespace BusinessLayer.Commands.Users;

public class ResetPasswordCommand : IRequest<ResponseModel<string>>
{
    public ResetPasswordModel ResetPasswordModel { get; set; } = new();
}