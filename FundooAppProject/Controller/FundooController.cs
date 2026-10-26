using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using NLog;
using RepositoryLayer.Entity;
using RepositoryLayer.Service;
using System.ComponentModel;
using System.Security.Claims;

namespace Fundoo.Controllers;

[ApiController] // Treat the class as a ASP.NET Core Web API controller
[Route("/api/[controller]")] //controller route , [controller] is a route token.
//Route defines the base URL for the controller [controller] -> replaced with the controller name i.e /api/Fundoo
public class FundooController : ControllerBase //inside asp.net namespace we inherit it to use the Ok(), BadRequest() methods
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
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
        ResponseModel<string> response = userBL.RegisterUserBL(registrationModel);
        if (response.IsSuccess)
        {
            logger.Info("User registration completed successfully");
            return Ok(response); //200
        }
        else
        {
            logger.Warn("User registration failed");
            return BadRequest(response); //400

        }
    }

    [HttpPost]
    [Route("login")] //POST /api/fundoo/login
    public IActionResult LoginUserBL(LoginModel login)  //json to object Model Binding
    //ASP.NET Core receives the reponse in JSON and ASP.NET Core automatically creates a LoginModel object and puts: email,password inside it. 
    {
        ResponseModel<LoginResponseModel> response = userBL.LoginUserBL(login);
        if (response.IsSuccess)
        {
            logger.Info("User login completed successfully");
            return Ok(response); //object to JSON sends back the result to client in the form of JSON
        }
        else
        {
            logger.Warn("User login failed..");
            return BadRequest(response);
        }
    }

    [HttpGet]
    public IActionResult Default()
    {
        Console.WriteLine("Application is running");
        return Ok("Fundoo Application is running");
    }
    
    //[Authorize] //That means a request to GET /api/Notes must be authenticated successfully
    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<UserEntity>> GetProfile(int userId) 
    {
        UserEntity? response = await userBL.GetProfileAsync(userId);
        if(response!=null)
        {
            return Ok(response);
        }
        else
        {
            return BadRequest("Profile not found");
        }
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
    [HttpGet("google-login")] //gets executed when user clicks "Continue with Google"
    public IActionResult GoogleLogin()
    {
        //Auth properties specifies what Fundoo should do after the external authentication process completes.
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/fundoo/google-complete"
        };
        //Challenge() asks ASP.NET Core to start the Google OIDC authentication flow, ASP.NET Core invokes the GoogleOIDC handler. The browser is redirected to Google, the user signs in, and Google redirects the browser back to Fundoo's OIDC callback(/signin-oidc)
        //controller starts the OAuth process using this Challenge method which takes properties and authentication scheme as GoogleOIDC
        return Challenge(properties, "GoogleOIDC"); 
    }

    //this endpoint will runs after the OIDC callback(/signin-oidc) succeeds, based on your configured RedirectUri.
    [AllowAnonymous]
    [HttpGet("google-complete")]
    public async Task<IActionResult> GoogleComplete()
    {
        var result = await HttpContext.AuthenticateAsync(
            "ExternalCookie"); //browser automatically attaches the applicable cookie to this new HTTP request.
        //AuthenticateAsync("ExternalCookie") asks ASP.NET Core to authenticate the current request using that specific scheme. If the temporary cookie is valid, the result contains the ClaimsPrincipal representing the external identity.
        //If your Google OIDC flow successfully signs in to the external cookie scheme
        //this result can contain the authenticated user's identity and claims, along with information about whether authentication succeeded.
        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized(new
            {
                Message = "Google sign-in failed."
            });
        }

        try
        {
            var principal = result.Principal; //gives your controller access to the claims associated with the external identity.

            //main identifier you should use to associate a Google login with a Fundoo account.
            //Google subject identifies the Google account
            var googleSubject =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;
            //The email identifies the email address
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

            //checks whether this Google account already exists
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

