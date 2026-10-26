using BusinessLayer.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using Middleware.Services.Encryption;

namespace BusinessLayer.Service;

public class UserBL : IUserBL
{
    public IUserRL userRL;
    public JwtService jwtService; // used to generate JWT Token
    public EmailClient client; ///used to Send an HTTP request to the separate Email project.
    private readonly IDistributedCache _cache;
    private readonly CustomPasswordHasher customPasswordHasher;
    public IConfiguration configuration;
    public UserBL(IUserRL userRL , JwtService jwtService,EmailClient client,IConfiguration configuration, IDistributedCache cache, CustomPasswordHasher customPasswordHasher)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
        this.client = client;
        this.configuration = configuration;
        _cache = cache;
        this.customPasswordHasher = customPasswordHasher;
    }


    //public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register)
    //{
    //    //redis logic
    //    return userRL.RegisterUserRL(register);
    //}
    public ResponseModel<string> RegisterUserBL(RegistrationModel model)
    {
        var (hash, salt) = customPasswordHasher.HashPassword(model.Password);

        var user = new UserEntity
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PhoneNumber = model.ContactNumber,

            Password = hash,
            Salt = salt
        };

        userRL.RegisterUserRL(user);

        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Registration successful.",
            Data = user.Email
        };
    }

    //public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login)
    //{

    //    UserEntity? userEntity =  userRL.LoginUserRL(login);

    //    ResponseModel<LoginResponseModel> rm = new ResponseModel<LoginResponseModel>();
    //    if (userEntity == null)
    //    {
    //        rm.IsSuccess = false;
    //        rm.Message = "login email or password is wrong";
    //        rm.Data = null;
    //    }
    //    else
    //    {
    //        rm.IsSuccess = true;
    //        rm.Message = "login success";
    //        rm.Data = new LoginResponseModel
    //        {
    //            Email = userEntity.Email,
    //            Token = jwtService.GenerateToken(userEntity) //generate a signed token of the user data (userEntity) passed in the parameter and then return the token to client in the form of LoginResponseModel
    //        };
    //    }
    //    return rm;
    //}


    public ResponseModel<LoginResponseModel> LoginUserBL(
        LoginModel loginModel)
    {
        UserEntity? user = userRL.FindUserByEmail(
            loginModel.Email
        );

        if (user == null)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = "Invalid email or password.",
                Data = null
            };
        }

        bool isPasswordValid = customPasswordHasher.VerifyPassword(
            loginModel.Password,
            user.Password,
            user.Salt
        );

        if (!isPasswordValid)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = "Invalid email or password.",
                Data = null
            };
        }

        return new ResponseModel<LoginResponseModel>
        {
            IsSuccess = true,
            Message = "Login successful.",
            Data = new LoginResponseModel
            {
                Email = user.Email,
                Token = jwtService.GenerateToken(user)
            }
        };
    }

    //Why does Forgot Password need Task?
    //Sending an email involves network I/O.Your Fundoo backend may need to wait for the email service to respond.
    //If that operation is asynchronous,await allows ASP.NET Core to release the current thread to do other work while the email request is pending.When the operation completes, the method continues and returns the result.

    //this return type Asynchronously returns your response model, but doesn't directly express HTTP status codes.


    public async Task<ResponseModel<string>> ForgetPasswordBL(ForgetPasswordModel model) 
    {
        //redis logic will be written here..
        UserEntity user = userRL.FindUserByEmail(model.Email);

        if (user == null)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "User not found.",
                Data = null
            };
        }

        //if user with the email exists then generate token for reset token so that we can reset the password using that token
        var token = jwtService.GenerateResetToken(user);

        //defining the EmailModel
        var emailModel = new EmailModel.Model.EmailModel()
        {
            To = user.Email,
            Subject = "Fundoo Password Reset",
            Body = $"""
                    Your password reset token is:
                    {token}
                    This token will expire in 15 minutes.
                    """
        };

        //Sends an HTTP request to EmailClient, which then separately calls Email project's controller, through HttpClient
        await client.SendEmail(emailModel);

        return new ResponseModel<string>
        {
            IsSuccess = true,
            Message = "Password reset instructions have been sent to the email",
            Data = token //here we are sending the reset token in response model so that we dont have to open the gmail and then copy the token for their again and again we can directly copy the token from the response body
        };

    }

    public async Task<ResponseModel<string>> ResetPassword(ResetPasswordModel model)
    {
        //check if new password and confirm password matches or not
        if(model.NewPassword != model.ConfirmPassword)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Passwords do not match.",
                Data = null
            };
        }
        
        //used to provide methods to read, create and validate the token 
        var tokenHandler = new JwtSecurityTokenHandler();

        //stores the bytes from key for creating the cryptographic signing key below in 
        var key = Encoding.UTF8.GetBytes( //This reads the configured JWT key and converts the string into bytes.
            configuration["Jwt:Key"]
        );

        try
        {
            //the supplied token will get validated inside ValidateToken() method which comes from JWTSecurityTokenHandler
            //ValidateToken() checks the token against your configured rules before returning the principal.
            //If a required check fails, it normally throws a token - validation exception, and your code jumps to the corresponding catch block instead of continuing with principal.
            var principal = tokenHandler.ValidateToken(
                model.Token,
                new TokenValidationParameters //the rules the token must satisfy.
                {
                    //This check ensures the incoming reset token matches the expected configuration by your application.
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, //This checks whether the token is within its validity period.
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(key)
                    //This creates the symmetric signing key used by ValidateToken() to verify that the reset token was signed with your application's secret.
                },
                out SecurityToken validatedToken
            );
            //After the token is validated, we will
            // accessing the claim's value using Claim principal and then verifiying that the token's claim matchess the actual claims written in JWT Service

            var purpose = principal.FindFirst("purpose")?.Value;
            //FindFirst("purpose") searches the validated principal's claims for the claim whose type is "purpose".
            if (purpose != "password-reset") //If the claim is missing or has a value other than "password-reset", your code rejects the token.
            {
                return new ResponseModel<string>
                {
                    IsSuccess = false,
                    Message = "Invalid reset token.",
                    Data = null
                };
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return new ResponseModel<string>
                {
                    IsSuccess = false,
                    Message = "Invalid reset token.",
                    Data = null
                };
            }

            int userId = int.Parse(userIdClaim.Value);
            UserEntity? user = userRL.GetUserById(userId);
            if (user == null)
            {
                return new ResponseModel<string>
                {
                    IsSuccess = false,
                    Message = "User not found.",
                    Data = null
                };
            }

            //the new hashed password is stores instead of a plain password  in the user Entity
            var passwordHasher = new PasswordHasher<UserEntity>();
            user.Password = passwordHasher.HashPassword(
                user,
                model.NewPassword
            );

            //after all the checks the new hashed password is finally updated in the DB through RL layer
            userRL.UpdatePassword(user);

            return new ResponseModel<string>
            {
                IsSuccess = true,
                Message = "Your password has been reset successfully.",
                Data = null
            };
        }

        //handles an expired token 
        catch (SecurityTokenExpiredException) 
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Reset token has expired.",
                Data = null
            };
        }
        //handles other token-related validation failures, such as an invalid signature or issuer/audience mismatch.
        catch (SecurityTokenException)
        {
            return new ResponseModel<string>
            {
                IsSuccess = false,
                Message = "Invalid reset token.",
                Data = null
            };
        }
    }

    public ResponseModel<LoginResponseModel> GoogleLoginBL(
    string email,
    string providerSubject,
    string firstName,
    string lastName)
    {
        try
        {
            //for Existing Google user
            UserEntity? user = userRL.FindUserByExternalLogin("Google", providerSubject);
            //here "Google" is the provider and "google-12345" is the providerSubject.

            if (user == null) //New Google user,Fundoo creates a user and stores the Google login association
            {
                user = userRL.CreateGoogleUserWithExternalLogin(
                    email,
                    firstName,
                    lastName,
                    providerSubject);
            }
            //providerSubject identifies the specific user account provided by that external service (here google)

            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = true,
                Message = "Google login successful.",
                Data = new LoginResponseModel
                {
                    Email = user.Email,
                    Token = jwtService.GenerateToken(user)
                }
            };
        }
        catch (InvalidOperationException ex)
        {
            return new ResponseModel<LoginResponseModel>
            {
                IsSuccess = false,
                Message = ex.Message,
                Data = null
            };
        }
    }

    //Redis Bussiness Logic
    public async Task<UserEntity?> GetProfileAsync(int userId)
    {
        string key = $"profile:{userId}";

        // 1. Check Redis
        string? cachedData = await _cache.GetStringAsync(key);

        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<UserEntity>(
                cachedData);
        }

        // 2. Cache miss: read from the repository
        UserEntity? user = userRL.GetUserById(userId);

        if (user == null)
            return null;

        // 3. Store the result in Redis
        string json = JsonSerializer.Serialize(user);

        await _cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(5)
            });

        return user;
    }






}