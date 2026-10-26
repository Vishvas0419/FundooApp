using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using EmailModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace EmailService.Service;
using EmailModel.Model;
using Interface;
using MailKit.Security;
using System.Diagnostics;


// this class Creates the email message and uses MailKit's SMTP client to connect, authenticate, send and then disconnect.
public class EmailService : IEmailService
{
    public IConfiguration configuration;
    public EmailService(IConfiguration configuration) {
        this.configuration = configuration;
    }
    public async Task SendEmail(EmailModel email)
    {
        //MimeMessage is the object that represents the actual email you want to send. It holds the email's contents and metadata in the form of an Object
        var message = new MimeMessage();
        //message format: ->
        //To
        //From
        //Subject
        //Body

        //sender's email (From), fetched from appsettings.json
        message.From.Add( 
            new MailboxAddress("Fundoo App", configuration["SmtpSettings:Username"])
        );

        //receiver's email (To)
        message.To.Add(
            MailboxAddress.Parse(email.To)
        );

        //Subject
        message.Subject = email.Subject;

        //Body
        message.Body = new TextPart("html")
        {
            Text = email.Body
        };

        //SMTP stands for Simple Mail Transfer Protocol, which actually send the email to the recipent
        //It is used to submit and relay outgoing email. It is different from HTTP, which Fundoo uses to communicate with the Email API.

        using var smtp = new SmtpClient();
        smtp.CheckCertificateRevocation = false;//whether to check the tls/ssl certificate or not 

        //Step 1 : MailKit connects to the configured SMTP server and port.
        await smtp.ConnectAsync(
            configuration["SmtpSettings:Host"],
            int.Parse(configuration["SmtpSettings:Port"]!),
            SecureSocketOptions.StartTls
        );

        //Step 2 : The application authenticates using the configured SMTP username and password.
        await smtp.AuthenticateAsync(
            configuration["SmtpSettings:Username"],
            configuration["SmtpSettings:Password"]
        );

        //Step 3 : MailKit submits the MimeMessage email object (message) to the SMTP server
        await smtp.SendAsync(message);

        //Step4 : The client closes the SMTP session cleanly.
        await smtp.DisconnectAsync(true);
    }
}
