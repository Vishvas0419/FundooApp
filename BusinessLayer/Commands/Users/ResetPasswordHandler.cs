using BusinessLayer.Service;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Middleware.Services;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Commands.Users;

public class ResetPasswordHandler
    : IRequestHandler<ResetPasswordCommand, ResponseModel<string>>
{
    private readonly IUserRL userRL;
    private readonly JwtService jwtService;
    private readonly PasswordHasher<UserEntity> passwordHasher;

    public ResetPasswordHandler(
        IUserRL userRL,
        JwtService jwtService,
        PasswordHasher<UserEntity> passwordHasher)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.passwordHasher = passwordHasher;
    }

    public Task<ResponseModel<string>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var model = request.ResetPasswordModel;

        if (model.NewPassword != model.ConfirmPassword)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Passwords do not match.",
                Data = null
            });
        }

        int userId;

        try
        {
            userId = jwtService.ValidatePasswordResetToken(model.Token);
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Reset token has expired.",
                Data = null
            });
        }
        catch (SecurityTokenException)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Invalid reset token.",
                Data = null
            });
        }

        var user = userRL.GetUserById(userId);

        if (user == null)
        {
            return Task.FromResult(new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "User not found.",
                Data = null
            });
        }

        user.Password = passwordHasher.HashPassword(
            user,
            model.NewPassword);

        userRL.UpdatePassword(user);

        return Task.FromResult(new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Your password has been reset successfully.",
            Data = null
        });
    }
}