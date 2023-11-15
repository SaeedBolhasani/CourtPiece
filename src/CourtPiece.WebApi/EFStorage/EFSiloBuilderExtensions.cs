using Orleans.Runtime;
using Orleans.Storage;

public static class EFSiloBuilderExtensions
{
    public static ISiloBuilder AddFileGrainStorage(
        this ISiloBuilder builder,
        string providerName)
    {
        return builder.ConfigureServices(services => services.AddFileGrainStorage(providerName));
    }
    public static IServiceCollection AddFileGrainStorage(
        this IServiceCollection services,
        string providerName)
    {
        return services.AddSingletonNamedService(providerName, EFGrainStorageFactory.Create)
            .AddSingletonNamedService(providerName,
                (p, n) =>
                    (ILifecycleParticipant<ISiloLifecycle>)p.GetRequiredServiceByName<IGrainStorage>(n));
    }
}