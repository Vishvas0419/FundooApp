namespace EmailController.Controller;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;



using EmailModel.Model;
using EmailService.Interface;
[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    private IEmailService emailService;
    public EmailController(IEmailService emaiService)
    {
        this.emailService = emaiService;
    }

    [HttpGet]
    public IActionResult defaultRoute()
    {
        return Ok("Email project is running");
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail(EmailModel model)
    {
        await emailService.SendEmail(model);

        return Ok("Email sent successfully");
    }
}