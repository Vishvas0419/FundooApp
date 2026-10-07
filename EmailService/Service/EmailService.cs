using EmailService.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;


namespace EmailService.Service;

using Interface;
using EmailModel.Model;
public class EmailService : IEmailService
{
    IConfiguration configuration;
    public EmailService() { }
    public async Task SendEMail(EmailModel emailModel)
    {

    }
}
