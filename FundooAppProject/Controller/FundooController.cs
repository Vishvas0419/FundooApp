using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Fundoo.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class FundooController : ControllerBase
{
    private IUserBL userBL;
    public FundooController(IUserBL userBL)
    {
        this.userBL = userBL;
    }

    [HttpPost]
    [Route("register")]
    public ResponseModel<RegistrationModel> RegisterUser(RegistrationModel registrationModel)
    {
        ResponseModel<RegistrationModel> responseModel = new ResponseModel<RegistrationModel>();
        RegistrationModel data =  userBL.RegisterUserBL(registrationModel);
        responseModel.IsSuccess = true;
        responseModel.Message = "Success";
        responseModel.Data = data;
        return responseModel;
    }

    public void Default()
    {
        Console.WriteLine("Application is running");
    }
    
}

