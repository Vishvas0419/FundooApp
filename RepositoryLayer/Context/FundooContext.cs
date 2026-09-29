using RepositoryLayer.Entity;

namespace RepositoryLayer.Context;
using Microsoft.EntityFrameworkCore;
public class FundooContext : DbContext
{
   public FundooContext(DbContextOptions<FundooContext> options) : base(options){}
   
  public DbSet<UserEntity> Users { get; set; }
  
   
}
