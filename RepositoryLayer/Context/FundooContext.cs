using RepositoryLayer.Entity;

namespace RepositoryLayer.Context;
using Microsoft.EntityFrameworkCore;
public class FundooContext : DbContext //we inherit it because to get configuration details of the db in appsettings.json through program.cs using dependency injection using AddDbContext<FundooContext>
{
   public FundooContext(DbContextOptions<FundooContext> options) : base(options){}

    //DbCOntextOptions will get the configuration settings for your DbContext from program.cs to DBContext
    public DbSet<UserEntity> Users { get; set; } //A database set containing UserEntity objects.
    //DbSet<> = DB set of entities, represents a collection of UserEntity records that EF Core can query and modify with the help of LINQ


    //so that if we want to to operations we can do 

    //FundooContext context;
    //context.Users.Add(UserEntity);
    //then Users.SaveChanges();
}
