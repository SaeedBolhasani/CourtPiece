using Orleans.Runtime;
using Microsoft.Extensions.Options;
using Orleans.Storage;

public static class FileSiloBuilderExtensions
{
    public static ISiloBuilder AddFileGrainStorage(
        this ISiloBuilder builder,
        string providerName,
        Action<FileGrainStorageOptions> options) =>
        builder.ConfigureServices(
            services => services.AddFileGrainStorage(
                providerName, options));

    public static IServiceCollection AddFileGrainStorage(
        this IServiceCollection services,
        string providerName,
        Action<FileGrainStorageOptions> options)
    {

        services.AddOptions<FileGrainStorageOptions>(providerName)
            .Configure(options);

        services.AddTransient<
            IPostConfigureOptions<FileGrainStorageOptions>,
            DefaultStorageProviderSerializerOptionsConfigurator<FileGrainStorageOptions>>();

        return services.AddSingletonNamedService(providerName, FileGrainStorageFactory.Create)
            .AddSingletonNamedService(providerName,
                (p, n) =>
                    (ILifecycleParticipant<ISiloLifecycle>)p.GetRequiredServiceByName<IGrainStorage>(n));
    }
}