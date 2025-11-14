using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AiManagementApp.Infrastructure;

public class AiManagementAppDbFactory : IDesignTimeDbContextFactory<AiManagementAppDb>
{
    public AiManagementAppDb CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        
        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        var cs = cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'Default Connection' ausente para design-time");
        
        var options = new DbContextOptionsBuilder<AiManagementAppDb>()
            .UseOracle(cs)
            .Options;
        
        return new AiManagementAppDb(options);
    }
}