using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.ComponentModel;

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
    
    [Authorize]
    [HttpGet]
    [Route("profile")]
    public IActionResult GetProfile() 
    {
        Console.WriteLine("Profile is displayed");
        return Ok("Profile is displayed");
    }


    [Authorize]
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

    [Authorize]
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


}

