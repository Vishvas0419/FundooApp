using BusinessLayer.Service;
using Middleware.Services.Encryption;
using MediatR;
using Middleware.Services;
using Models;
using RepositoryLayer.Interface;

namespace BusinessLayer.Queries.Users;

public class LoginUserHandler
    : IRequestHandler<LoginUserQuery, ResponseModel<LoginResponseModel>>
{
    private readonly IUserRL userRL;
    private readonly CustomPasswordHasher _passwordHasher;
    private readonly JwtService jwtService;

    public LoginUserHandler(
        IUserRL userRL,
        CustomPasswordHasher passwordHasher,
        JwtService jwtService)
    {
        this.userRL = userRL;
        this._passwordHasher = passwordHasher;
        this.jwtService = jwtService;
    }

    public Task<ResponseModel<LoginResponseModel>> Handle(
        LoginUserQuery request,
        CancellationToken cancellationToken)
    {
        var model = request.LoginModel;

        var user = userRL.FindUserByEmail(model.Email);

        if (user == null ||
            !_passwordHasher.VerifyPassword(
                model.Password,
                user.Password,
                user.Salt))
        {
            return Task.FromResult(
                new ResponseModel<LoginResponseModel>
                {
                    IsSuccess = false,
                    Message = "Invalid email or password.",
                    Data = null
                });
        }

        var response = new ResponseModel<LoginResponseModel>
        {
            IsSuccess = true,
            Message = "Login successful.",
            Data = new LoginResponseModel
            {
                Email = user.Email,
                Token = jwtService.GenerateToken(user)
            }
        };

        return Task.FromResult(response);
    }
}