using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CourtPiece.IntegrationTest.Infrastructure
{
    public class TestingWebAppFactory<TEntryPoint> : WebApplicationFactory<Program> where TEntryPoint : Program
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(async services =>
            {
                RemoveService(services, typeof(DbContextOptions<ApplicationDbContext>));
                RemoveService(services, typeof(CardProvider));

                var cardProvider = new Mock<ICardProvider>();
                services.AddSingleton(cardProvider);
                services.AddSingleton(typeof(ICardProvider), cardProvider.Object);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryTest");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                using var appContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    appContext.Database.EnsureDeleted();
                }
                catch (Exception ex)
                {
                    //Log errors or do anything you think it's needed
                    throw;
                }

            });
        }

        private static void RemoveService(IServiceCollection services, Type type)
        {
            var descriptor = services.SingleOrDefault(d => d.ImplementationType == type || d.ServiceType == type);

            if (descriptor != null)
                services.Remove(descriptor);
        }
    }
}