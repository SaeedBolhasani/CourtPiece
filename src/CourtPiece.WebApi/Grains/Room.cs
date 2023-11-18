using CourtPiece.Common.Model;
using Microsoft.AspNetCore.SignalR;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;

[StorageProvider(ProviderName = "File")]
public class Room : Grain<RoomState>, IRoom
{
    private IHubContext<RoomHub> playerHub;

    public async Task Action(Immutable<ICard> card, IPlayer player)
    {
        if (player.GetPrimaryKeyLong() != this.State.TurnId || this.State.TrumpSuit == null)
        {
            await this.playerHub.Clients.User(State.TrumpCaller.ToString()).SendAsync("Error", "It is not Your Turn.");
            return;
        }

        if (this.State.PlayerCards[player.GetPrimaryKeyLong()].Contains(card.Value))
        {
            await this.playerHub.Clients.User(State.TrumpCaller.ToString()).SendAsync("Error", "You have not this card.");
            return;
        }

        this.State.CurrentTricks.Add(card.Value);

        if (this.State.CurrentTricks.Count == 4)
        {

        }


        this.State.TurnId = (State.TurnId + 1) % 4;

    }

    public async Task<JoinPlayerResult> JoinPlayer(IPlayer player)
    {        
        if (State.PlayerIds.Count >= 4)
        {
            return JoinPlayerResult.Error("Room is full.", this.GetPrimaryKey());
        }

        var userId = player.GetPrimaryKeyLong();
        if (State.PlayerIds.Contains(userId))
        {
            return JoinPlayerResult.Error("User has been joined already!", this.GetPrimaryKey());
        }
        
        await this.playerHub.Clients.Group(this.GetPrimaryKey().ToString()).SendAsync("Room", "User Joined " + userId);
        this.State.PlayerIds.Add(userId);
        await this.playerHub.Clients.User(userId.ToString()).SendAsync("YouJoined", this.GetPrimaryKey());


        if (this.State.PlayerIds.Count == 4)
        {
            State.TurnId = State.PlayerIds[0];
            State.TrumpCaller = State.PlayerIds[0];

            var cards = GetCards().Chunk(13).ToArray();
            var i = 0;
            foreach (var item in State.PlayerIds)
            {
                this.State.PlayerCards.Add((item, cards[i].ToList()));
                i++;
            }
            await this.playerHub.Clients.User(State.TrumpCaller.ToString()).SendAsync("ChooseTrumpSuit", cards[0].Take(5).ToArray());

            return JoinPlayerResult.GameStarted(this.GetPrimaryKey());
        }

        return JoinPlayerResult.Success(this.GetPrimaryKey());
    }


    public async Task ChooseTrumpSuit(CardTypes trumpSuit, IPlayer player)
    {
        if (player.GetPrimaryKeyLong() != this.State.TurnId || player.GetPrimaryKeyLong() != this.State.TrumpCaller || this.State.TrumpSuit != null)
        {
            await this.playerHub.Clients.User(State.TrumpCaller.ToString()).SendAsync("Error", "It is not Your Turn.");
            return;
        }

        this.State.TrumpSuit = trumpSuit;
        await this.playerHub.Clients.Group(this.GetPrimaryKey().ToString()).SendAsync("TrumpSuit", trumpSuit);
        
        foreach (var (playerId,cards) in State.PlayerCards)
        {
            await this.playerHub.Clients.User(playerId.ToString()).SendAsync("Cards", cards);
        }
    }


    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.playerHub = this.ServiceProvider.GetRequiredService<IHubContext<RoomHub>>();
        await ReadStateAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await this.WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    // 8♥ 


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

