using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration; //Reads settings such as Jwt:Key, Jwt:Issuer and Jwt:Audience.
using Microsoft.IdentityModel.Tokens; //Provides signing keys, signing credentials and the HS256 algorithm identifier.
using RepositoryLayer.Entity;
using System.IdentityModel.Tokens.Jwt; //Creates JWT objects and serializes them into token strings.
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace BusinessLayer.Service;

public class JwtService
{
    //IConfiguration is an ASP.NET Core interface that allows your application to read configuration values from sources such as appsettings.json, environment variables and other configured providers.
   private readonly IConfiguration configuration;
   public JwtService(IConfiguration configuration)
   {
      this.configuration = configuration;
   }
   
   public string GenerateToken(UserEntity user) //generates a token for normal authentication, such as accessing protected APIs after login. The method receives the user object so it can put selected user information into the token's claims.
    {
      var claims = new [] //creates payload
      {
         new Claim(ClaimTypes.Email, user.Email), 
         new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) //here NameIdentifier is the UserId
      };//A claim is a piece of information about a subject/user , such as a user ID, email address, role or permission just like a Payload

        //creating a signature
        //secret key is a unique private key that only backend knows
        //This line takes the secret key from your application configuration and converts it into a cryptographic key object.
        var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
      );

        //This tells the JWT library two things : Which key to use: key and Which signing algorithm to use: HmacSha256.
        //binds your key with some hash recipe using HS256 to generate a signature
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); //HS256 in JWT headers, responsible for the creation of header and signature

      var token = new JwtSecurityToken(
         issuer: configuration["Jwt:Issuer"], //part of payload
         audience: configuration["Jwt:Audience"], //part of payload
         claims: claims, //part of payload
         expires: DateTime.UtcNow.AddHours(1), //expires after 1 hour, part of payload
         signingCredentials: credentials //This supplies the signing key and algorithm to the JWT object so that the token can be signed when serialized.   
      );
      
        return new JwtSecurityTokenHandler().WriteToken(token); //The JwtSecurityToken created above is a C# object. Your frontend needs a serialized token string that it can receive in an HTTP response and send in an authorization header.
//JwtSecurityTokenHandler handles the serialization.
//WriteToken(token) serializes the JWT into its compact representation:
   }

    
    public string GenerateResetToken(UserEntity user) //generates a short-lived token intended for the password-reset process.
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
            new Claim("purpose","password-reset") //purpose=password-reset is a custom claim inside your JWT. It does not perform the reset by itself; it allows your backend to recognize and enforce that the token is intended for password resetting.
        };

        //generates a reset JWT and signs it using Jwt:Key.
        var key = new SymmetricSecurityKey(
           Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
        );

        var credentials = new SigningCredentials( //specifies the signing key and algorithm.
           key, SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
           issuer: configuration["Jwt:Issuer"],
           audience: configuration["Jwt:Audience"],
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(15),
           signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token); //handles the serialization : it converts the token from obj to string in its token form because the Token property should receive the token as string so that i can do  
    }

    public int ValidatePasswordResetToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(
           configuration["Jwt:Key"]
           ?? throw new InvalidOperationException("JWT key is not configured.")
        );

        var principal = tokenHandler.ValidateToken(
           token,
           new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,

               ValidIssuer = configuration["Jwt:Issuer"],
               ValidAudience = configuration["Jwt:Audience"],

               IssuerSigningKey = new SymmetricSecurityKey(key)
           },
           out _
        );

        var purpose = principal.FindFirst("purpose")?.Value;

        if (purpose != "password-reset")
        {
            throw new SecurityTokenException("Invalid reset token purpose.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new SecurityTokenException("Invalid user ID claim.");
        }

        return userId;
    }



}