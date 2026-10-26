using BusinessLayer.Interface;
using BusinessLayer.Service;
using EmailService.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Middleware.ExceptionHandling;
using Middleware.Services.Encryption;
using NLog.Web;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.Runtime.Intrinsics.X86;
using System.Text;

NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;
NLog.Common.InternalLogger.LogToConsole = true;

const string GoogleOidcScheme = "GoogleOIDC"; //identifies the OpenID Connect handler that communicates with Google.
const string ExternalCookieScheme = "ExternalCookie";//identifies the temporary cookie scheme that stores the validated external identity while Fundoo completes its login logic.

var builder = WebApplication.CreateBuilder(args);



builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserBL , UserBL>();
// this means Whenever the application needs an IUserBL, use UserBL as its implementation to create obj of UserBL
builder.Services.AddScoped<IUserRL , UserRL>();
builder.Services.AddScoped<INotesBL, NotesBL>();
builder.Services.AddScoped<INotesRL, NotesRL>();
builder.Services.AddScoped<IEmailService, EmailService.Service.EmailService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordHasher<UserEntity>>();
builder.Services.AddScoped<EmailClient>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<CustomPasswordHasher>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


//AddDbContext registers the FundooContext with DI and configures EF Core to use PostgreSQL through Npgsql.
builder.Services.AddDbContext<FundooContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("FundooConnection")); //this will access the config details (host, portnumber,db name, username,password) of your DB provider postgres from appsettings.json
});

//UseNpgsql Tells EF Core to Use PostgreSQL as the database provider.


//CORS configuration
//supplies the headers that tell the browser whether it may share the response with your frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:8080") //Access-Control-Allow-Origin set automatically by asp.net
            .AllowAnyHeader() //Permits request headers such as Content-Type and Authorization.
            .AllowAnyMethod(); //Permits http methods such as GET, POST, PUT, PATCH, and DELETE
    });
});

//Checks the signing key, issuer, audience and token lifetime; a valid token establishes HttpContext.User.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //Selects JWT Bearer authentication, conventionally named Bearer
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => //Configures the handler that reads and validates incoming bearer tokens.
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            )
        };
    });


//adding OAuth Configuration
builder.Services.AddAuthentication()
    .AddCookie(ExternalCookieScheme, options =>
    {
        options.Cookie.Name = "Fundoo.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Local HTTP development only.
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    })
    //This configures Google as your OpenID Connect identity provider.
    //when Challenge(properties, "GoogleOIDC") executes, ASP.NET Core looks up the registered scheme named "GoogleOIDC" and invokes its OpenID Connect handler.
    .AddOpenIdConnect(GoogleOidcScheme, options =>
    {
        options.SignInScheme = ExternalCookieScheme; //Tells the OIDC handler where to place the validated external identity
        options.Authority = "https://accounts.google.com"; //Identifies Google as the OIDC provider

        options.ClientId =
            builder.Configuration["Authentication:Google:ClientId"]!; //Identifies your registered Fundoo application to Google

        options.ClientSecret =
            builder.Configuration["Authentication:Google:ClientSecret"]!; //Helps your backend authenticate itself when exchanging the authorization code

        //handled by ASP.NET Core's OpenID Connect middleware.
        options.CallbackPath = "/signin-oidc"; //The endpoint where the OIDC handler receives Google's response
        options.ResponseType = "code"; //Uses the authorization-code flow rather than returning an access token directly in the initial response
        options.UsePkce = true;
        options.SaveTokens = false; //Doesn't save the provider tokens in the authentication properties

        options.Scope.Clear();

        //these are the "Scopes" or user info we are requesting from Google for external User Login
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.GetClaimsFromUserInfoEndpoint = true; //Allows the handler to retrieve additional claims from the provider's UserInfo endpoint
    }); 

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


//redis configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = 
                builder.Configuration.GetConnectionString("Redis");

    options.InstanceName = "Fundoo:";
});

var app = builder.Build();

//app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowReactApp"); //CORS should be applied after routing and before authorization/endpoints as appropriate for the configured pipeline

app.UseAuthentication(); //invokes the JWT Bearer handler configured by AddJwtBearer() and Validates the JWT and establishes the user's identity.
app.UseAuthorization(); //Checks the authorization requirements attached through [Authorize].
//Authentication should run before authorization so that authorization can use the identity established by authentication.

//app.UseHttpsRedirection();

// Map Controllers = Controller endpoints become available and ASP.NET Core can route HTTP requests to them
app.MapControllers(); 

app.Run();