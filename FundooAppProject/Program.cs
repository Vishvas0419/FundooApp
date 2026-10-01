using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Identity;
using RepositoryLayer.Context;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;

var builder = WebApplication.CreateBuilder(args);

// Add controller support
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserBL , UserBL>();
builder.Services.AddScoped<IUserRL , UserRL>();
builder.Services.AddScoped<PasswordHasher<UserEntity>>();
builder.Services.AddDbContext<FundooContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("FundooConnection"));
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map Controllers
app.MapControllers();

app.Run();