using Orleans.Providers;

[StorageProvider(ProviderName = StorageNames.DefaultEFStorageName)]
public class Player : Grain, IPlayer
{
    private AppUser user;

    public Task<string> GetFullName()
    {
        return Task.FromResult(user.UserName!);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var dbContext = this.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        this.user = (await dbContext.Users.FindAsync((int)this.GetPrimaryKeyLong(), cancellationToken))!;
        await base.OnActivateAsync(cancellationToken);
    }


}


