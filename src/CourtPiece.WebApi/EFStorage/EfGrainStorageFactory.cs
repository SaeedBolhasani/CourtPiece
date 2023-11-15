internal static class EFGrainStorageFactory
{
    internal static IGrainStorage Create(
        IServiceProvider services, string name)
    {
        return ActivatorUtilities.CreateInstance<EFGrainStorage>(
            services,
            name,
            services.GetRequiredService<Func<ApplicationDbContext>>());
    }
}
