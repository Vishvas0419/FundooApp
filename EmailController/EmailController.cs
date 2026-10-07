using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EmailController.Controller;

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

    [HttpPost]
    public Task<IActionResult> SendEmail(EmailModel emailModel)
    {

    }
}