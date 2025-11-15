using AiManagementApp.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AiManagementApp.Tests;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AiManagementAppDb>) ||
                d.ServiceType == typeof(AiManagementAppDb));

            foreach (var descriptor in dbContextDescriptors.ToArray())
            {
                services.Remove(descriptor);
            }
            
            Console.WriteLine(">> [TEST] Usando InMemoryDatabase para os testes");
            services.AddDbContext<AiManagementAppDb>(options =>
                options.UseInMemoryDatabase("AiManagementAppDb"));
            
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AiManagementAppDb>();
            db.Database.EnsureCreated();
        });
    }
}