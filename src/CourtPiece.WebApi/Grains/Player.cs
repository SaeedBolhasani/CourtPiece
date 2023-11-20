using CourtPiece.Common.Model;
using CourtPiece.WebApi.Grains;
using Orleans.Providers;

[StorageProvider(ProviderName = StorageNames.DefaultEFStorageName)]

public class Player : Grain, IPlayer
{

    public Task Action(Card card)
    {
        return Task.CompletedTask;

    }

    public async Task Join(IRoom room)
    {
        // await room.AddPlayer(this);
    }



}


