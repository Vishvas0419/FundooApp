using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using System.Runtime.Intrinsics.X86;
using System.Text;


using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;


const string ExternalCookieScheme = "ExternalCookie";
const string GoogleOidcScheme = "GoogleOIDC";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserBL , UserBL>();
// this means Whenever the application needs an IUserBL, use UserBL as its implementation to create obj of UserBL
builder.Services.AddScoped<IUserRL , UserRL>();
builder.Services.AddScoped<INotesBL, NotesBL>();
builder.Services.AddScoped<INotesRL, NotesRL>();


builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordHasher<UserEntity>>();

builder.Services.AddScoped<EmailClient>();
//builder.Services.AddScoped<HttpClient>();
builder.Services.AddHttpClient();

//AddDbContext registers the FundooContext with DI and configures EF Core to use PostgreSQL through Npgsql.
builder.Services.AddDbContext<FundooContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("FundooConnection")); //this will access the config details (host, portnumber,db name, username,password) of your DB provider postgres from appsettings.json
});
//UseNpgsql Tells EF Core to Use PostgreSQL as the database provider.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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
    .AddOpenIdConnect(GoogleOidcScheme, options =>
    {
        options.SignInScheme = ExternalCookieScheme;
        options.Authority = "https://accounts.google.com";

        options.ClientId =
            builder.Configuration["Authentication:Google:ClientId"]!;

        options.ClientSecret =
            builder.Configuration["Authentication:Google:ClientSecret"]!;

        options.CallbackPath = "/signin-oidc";
        options.ResponseType = "code";
        options.UsePkce = true;
        options.SaveTokens = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.GetClaimsFromUserInfoEndpoint = true;
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



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
//Authentication should run before authorization so that authorization can use the identity established by authentication.

//app.UseHttpsRedirection();

// Map Controllers = Controller endpoints become available and ASP.NET Core can route HTTP requests to them
app.MapControllers(); 

app.Run();