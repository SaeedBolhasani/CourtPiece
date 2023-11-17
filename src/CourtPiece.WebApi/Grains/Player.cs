using CourtPiece.Common.Model;
using Orleans.Providers;
using Orleans.Streams;

[StorageProvider(ProviderName = "File")]
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


