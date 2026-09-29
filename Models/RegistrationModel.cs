namespace Models;

public class RegistrationModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }

    public RegistrationModel(string firstName, string lastName, string userName, string password, string contactNumber,
        string email)
    {
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Password = password;
        ContactNumber = contactNumber;
        Email = email;
    }
}