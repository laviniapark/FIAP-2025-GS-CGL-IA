using AiManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AiManagementApp.Infrastructure;

public class AiManagementDb: DbContext
{
    public DbSet<AiLog> AiLogs { get; set; }
    
    public AiManagementDb(DbContextOptions<AiManagementDb> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiManagementDb).Assembly);
        }
}