using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.ComponentModel;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Fundoo.Controllers;

[ApiController] // Treat the class as a ASP.NET Core Web API controller
[Route("/api/[controller]")] //controller route , [controller] is a route token.
//Route defines the base URL for the controller [controller] -> replaced with the controller name i.e /api/Fundoo
public class FundooController : ControllerBase //inside asp.net namespace we inherit it to use the Ok(), BadRequest() methods
{
    private IUserBL userBL;
    public FundooController(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    [HttpPost] //type of client req 
    [Route("register")] //method route final url final http method: POST /api/Fundoo/register
    //if user hits this api call RegisterUser will gets executed
    public IActionResult RegisterUser(RegistrationModel registrationModel)
    {
        ResponseModel<RegistrationModel> response = userBL.RegisterUserBL(registrationModel);
        if (response.IsSuccess)
        {
            return Ok(response); //200
        }
        return BadRequest(response); //400
    }

    [HttpPost]
    [Route("login")] //POST /api/fundoo/loginz
    public IActionResult LoginUserBL(LoginModel login)  //json to object Model Binding
    //ASP.NET Core receives the reponse in JSON and ASP.NET Core automatically creates a LoginModel object and puts: email,password inside it. 
    {
        ResponseModel<LoginResponseModel> response = userBL.LoginUserBL(login);
        if (response.IsSuccess)
        {
            return Ok(response); //object to JSON sends back the result to client in the form of JSON
        }
        return BadRequest(response);
    }

    [HttpGet]
    public IActionResult Default()
    {
        Console.WriteLine("Application is running");
        return Ok("Fundoo Application is running");
    }
    
    [Authorize] //That means a request to GET /api/Notes must be authenticated successfully
    [HttpGet]
    [Route("profile")]
    public IActionResult GetProfile() 
    {
        Console.WriteLine("Profile is displayed");
        return Ok("Profile is displayed");
    }

    //[Authorize]
    [HttpPost]
    [Route("forget-password")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordModel forgetPasswordModel)
    {
        ResponseModel<string> responseModel = await userBL.ForgetPasswordBL(forgetPasswordModel);
        if (responseModel.IsSuccess)
        {
            return Ok(responseModel);
        }
        return BadRequest(responseModel);   
    }

    //[Authorize]
    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        ResponseModel<string> responseModel = await userBL.ResetPassword(resetPasswordModel);
        if (responseModel.IsSuccess)
        {
            return Ok(responseModel);
        }
        return BadRequest(responseModel);
    }


    [AllowAnonymous]
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/fundoo/google-complete"
        };

        return Challenge(properties, "GoogleOIDC");
    }


    [AllowAnonymous]
    [HttpGet("google-complete")]
    public async Task<IActionResult> GoogleComplete()
    {
        var result = await HttpContext.AuthenticateAsync(
            "ExternalCookie");

        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized(new
            {
                Message = "Google sign-in failed."
            });
        }

        try
        {
            var principal = result.Principal;

            var googleSubject =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;

            var email =
                principal.FindFirst(ClaimTypes.Email)?.Value
                ?? principal.FindFirst("email")?.Value;

            var emailVerified =
                principal.FindFirst("email_verified")?.Value;

            var firstName =
                principal.FindFirst(ClaimTypes.GivenName)?.Value
                ?? principal.FindFirst("given_name")?.Value
                ?? string.Empty;

            var lastName =
                principal.FindFirst(ClaimTypes.Surname)?.Value
                ?? principal.FindFirst("family_name")?.Value
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(googleSubject) ||
                string.IsNullOrWhiteSpace(email) ||
                !string.Equals(
                    emailVerified,
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new
                {
                    Message =
                        "Google identity or verified email is missing."
                });
            }

            var response = userBL.GoogleLoginBL(
                email,
                googleSubject,
                firstName,
                lastName);

            if (!response.IsSuccess)
            {
                return Conflict(response);
            }

            return Ok(response);
        }
        finally
        {
            await HttpContext.SignOutAsync("ExternalCookie");
        }
    }



}

