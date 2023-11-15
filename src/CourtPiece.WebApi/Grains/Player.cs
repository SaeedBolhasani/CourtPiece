using CourtPiece.Common.Model;
using Orleans.Providers;
using Orleans.Streams;

[StorageProvider(ProviderName = "File")]
public class Player : Grain, IPlayer
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var ss = this.ServiceProvider.GetRequiredService<IClusterClient>();
        var streamProvider = ss.GetStreamProvider("test");
        await streamProvider.GetStream<string>("test").OnNextAsync(this.GetPrimaryKeyString());

        await base.OnActivateAsync(cancellationToken);
    }
    public Task Action(Card card)
    {
        return Task.CompletedTask;

    }

    public async Task Join(IRoom room)
    {
        await room.AddPlayer(this);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        //await this.WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }


}


