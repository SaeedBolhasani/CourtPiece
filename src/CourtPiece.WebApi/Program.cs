
using CourtPiece.WebApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    // options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddDbContext<ApplicationDbContext>(i => i.UseSqlServer(builder.Configuration.GetConnectionString("Default"), j => j.MigrationsAssembly("CourtPiece.WebApi")));


builder.Services.AddSingleton<Func<ApplicationDbContext>>(i =>
{
    return () => i.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
});

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Host.ConfigureOrleans();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();


builder.Services.ConfigureIdentity(builder.Configuration);



var app = builder.Build();

//app.Services.CreateScope().ServiceProvider.GetService<ApplicationDbContext>()!.Database.Migrate();

app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Court Piece API v1");
});
app.MapControllers();
app.MapHub<RoomHub>("/chatHub");
app.Run();

public partial class Program
{

}