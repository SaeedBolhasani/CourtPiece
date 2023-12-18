
using CourtPiece.WebApi;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Versioning;
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
builder.Services.AddVersionedApiExplorer(
    options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver"));
});

builder.Services.ConfigureIdentity(builder.Configuration);



var app = builder.Build();

//app.Services.CreateScope().ServiceProvider.GetService<ApplicationDbContext>()!.Database.Migrate();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/html";

        await context.Response.WriteAsync("<html lang=\"en\"><body>\r\n");
        await context.Response.WriteAsync("ERROR!<br><br>\r\n");

        var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature?.Error is not null)
        {
            await context.Response.WriteAsync(
                                      "File error thrown!<br><br>\r\n");
        }

        await context.Response.WriteAsync(
                                      "<a href=\"/\">Home</a><br>\r\n");
        await context.Response.WriteAsync("</body></html>\r\n");
        await context.Response.WriteAsync(new string(' ', 512));
    });
});
app.UseHsts();
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