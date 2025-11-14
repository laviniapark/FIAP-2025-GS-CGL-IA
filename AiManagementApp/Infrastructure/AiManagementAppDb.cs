using AiManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AiManagementApp.Infrastructure;

public class AiManagementAppDb: DbContext
{
    public DbSet<AiLog> AiLogs { get; set; }
    
    public AiManagementAppDb(DbContextOptions<AiManagementAppDb> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiManagementAppDb).Assembly);
        }
}