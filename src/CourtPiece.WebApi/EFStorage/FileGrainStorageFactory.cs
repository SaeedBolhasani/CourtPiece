using Microsoft.Extensions.Options;
using Orleans.Storage;
using Orleans.Configuration.Overrides;
using Microsoft.EntityFrameworkCore;

internal static class FileGrainStorageFactory
{
    internal static IGrainStorage Create(
        IServiceProvider services, string name)
    {
        var optionsMonitor =
            services.GetRequiredService<IOptionsMonitor<FileGrainStorageOptions>>();

        return ActivatorUtilities.CreateInstance<EFGrainStorage>(
            services,
            name,
            services.GetRequiredService<Func<ApplicationDbContext>>());
    }
}
