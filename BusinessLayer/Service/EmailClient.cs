using System.Net.Http.Json;
namespace BusinessLayer.Service;
using EmailService.Interface;
using EmailModel.Model;

public class EmailClient
{
    public IEmailService emailService;
    //HttpClient is a .NET class for making HTTP requests to another server. Here, the other server is your separate Email application.
    public HttpClient Client { get; } 
    public EmailClient(HttpClient client)
    {
        Client = client;
    }

    public async Task<bool> SendEmail(EmailModel model) //this model includes : To,Subject,Data which we will be sending to the email recipent (To)
    {
        //await Client.PostAsJsonAsync("https://localhost:7178/email/send", model);
        return true;
        //This method serializes your EmailModel into JSON and sends it to your Email Controller in the form of a HTTP POST request
    }

    // this is service-to-service communication over HTTP
}