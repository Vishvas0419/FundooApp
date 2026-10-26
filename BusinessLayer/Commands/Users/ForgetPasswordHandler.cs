using MediatR;
using Models;
using RepositoryLayer.Interface;
using BusinessLayer.Service;
using Middleware.Services;
using EmailModel.Model;

namespace BusinessLayer.Commands.Users;

public class ForgetPasswordHandler
    : IRequestHandler<ForgetPasswordCommand, ResponseModel<string>>
{
    private readonly IUserRL userRL;
    private readonly JwtService jwtService;
    private readonly EmailClient emailClient;

    public ForgetPasswordHandler(
        IUserRL userRL,
        JwtService jwtService,
        EmailClient emailClient)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.emailClient = emailClient;
    }

    public async Task<ResponseModel<string>> Handle(
        ForgetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var model = request.ForgetPasswordModel;

        var user = userRL.FindUserByEmail(model.Email);

        if (user == null)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Unable to process the password reset request.",
                Data = null
            };
        }

        var token = jwtService.GenerateResetToken(user);

        var emailModel = new EmailModel.Model.EmailModel
        {
            To = user.Email,
            Subject = "Fundoo Password Reset",
            Body = $"""
                    Your password reset token is:
                    {token}

                    This token will expire in 15 minutes.
                    """
        };

        bool emailSent = await emailClient.SendEmail(emailModel);

        if (!emailSent)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Unable to process the password reset request.",
                Data = null
            };
        }

        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "If an account exists, password reset instructions have been sent.",
            Data = null
        };
    }
}