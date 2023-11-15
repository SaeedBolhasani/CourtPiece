using CourtPiece.Common.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Utilities;
using System;

public class RoomManagerState
{
    public List<Guid> EmptyRoomIds { get; set; } = new List<Guid>();
}

public interface IRoomManager: IGrainWithIntegerKey
{
    Task<IRoom> CreateRoom();

    Task MarkAsFull(IRoom room);
}

[ImplicitStreamSubscription("test")]
[StorageProvider(ProviderName = "File")]
public class RoomManager : Grain<RoomManagerState>, IRoomManager
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var ss = this.ServiceProvider.GetRequiredService<IClusterClient>();
        var streamProvider = ss.GetStreamProvider("test");
        await streamProvider.GetStream<string>("test").SubscribeAsync(async i =>
        {
            await Task.CompletedTask;
        });
        await base.OnActivateAsync(cancellationToken);
    }
    public Task<IRoom> CreateRoom()
    {
        throw new NotImplementedException();
    }

    public Task MarkAsFull(IRoom room)
    {
        this.State.EmptyRoomIds.Add(room.GetPrimaryKey());
        return Task.CompletedTask;
    }
}

[StorageProvider(ProviderName = "File")]

public class Room : Grain<RoomState>, IRoom
{
    private IHubContext<PlayerHub> playerHub;
    private IStreamProvider streamProvider;
    private IAsyncStream<string> stream;

    public async Task Action(Immutable<ICard> card, IPlayer player)
    {
        //throw new NotImplementedException();
        var cc = State.PlayerCards[1].cards[0] == card.Value;
    }

    public async Task AddPlayer(IPlayer player)
    {
        if (State.PlayerIds.Count >= 4)
        {
            throw new Exception();
        }
        if (State.PlayerIds.Contains(player.GetGrainId().GetIntegerKey()))
        {
            throw new Exception();
        }

        await this.playerHub.SentToRoom(this.GetGrainId().GetGuidKey(), "User Joined " + player.GetGrainId().GetIntegerKey());


        this.State.PlayerIds.Add(player.GetGrainId().GetIntegerKey());

        if (this.State.PlayerIds.Count == 4)
        {
            //var roomManager = GrainFactory.GetGrain<IRoomManager>(0);
            //await roomManager.MarkAsFull(this);
            State.TurnId = State.PlayerIds[0];
            State.TrumpCaller = State.PlayerIds[0];
            //State.TrumpSuit = 

            var cards = GetCards().Chunk(13).ToArray();
            var i = 0;
            foreach (var item in State.PlayerIds)
            {
                this.State.PlayerCards.Add((item, cards[i].ToList()));

                await this.playerHub.SendToUser(item, cards[i]);
                i++;
            }
        }
    }


    public async Task ChooseTrumpSuit(CardTypes trumpSuit, IPlayer player)
    {
        if (player.GetPrimaryKeyLong() != this.State.TurnId || player.GetPrimaryKeyLong() != this.State.TrumpCaller || this.State.TrumpSuit != null)
            throw new Exception();

        this.State.TrumpSuit = trumpSuit;
        await this.playerHub.SentToRoom(this.GetPrimaryKey(), trumpSuit);

    }


    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.playerHub = this.ServiceProvider.GetRequiredService<IHubContext<PlayerHub>>();       
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await this.WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    private static Card[] GetCards()
    {
        var cards = new Card[52];

        var j = 0;
        foreach (var cardType in Enum.GetValues<CardTypes>())
        {
            for (var i = 0; i < 13; i++)
            {
                cards[j * 13 + i] = new Card
                {
                    Value = (byte)(i + 1),
                    Type = cardType

                };
            }
            j++;
        }
        var random = new Random();

        return cards.OrderBy(i => random.Next()).ToArray();
    }
}

