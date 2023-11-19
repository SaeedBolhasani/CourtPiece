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
        long playerId = player.GetPrimaryKeyLong();

        if (State.IsFinished)
        {
            await this.playerHub.Clients.User(playerId.ToString()).SendAsync("Error", "Gave is over!");
            return;
        }


        Hand currentHand = this.State.CurrentHand;

        if (playerId != currentHand.CurrentTrick.TurnId || currentHand.TrumpSuit == null)
        {
            await this.playerHub.Clients.User(playerId.ToString()).SendAsync("Error", "It is not Your Turn.");
            return;
        }

        if (!currentHand.PlayerCards[playerId].Contains(card.Value))
        {
            await this.playerHub.Clients.User(playerId.ToString()).SendAsync("Error", "You have not this card.");
            return;
        }

        if (currentHand.CurrentTrick.OriginalSuit is null)
        {
            currentHand.CurrentTrick.OriginalSuit = card.Value.Type;
        }

        if (currentHand.CurrentTrick.OriginalSuit != card.Value.Type && currentHand.PlayerCards[playerId].Any(i => i.Type == currentHand.CurrentTrick.OriginalSuit))
        {
            await this.playerHub.Clients.User(playerId.ToString()).SendAsync("Error", "You can not play this card.");
            return;
        }

        currentHand.PlayerCards[playerId].Remove(card.Value);

        currentHand.CurrentTrick.Cards.Add(playerId, card.Value);

        await this.playerHub.Clients.Group(this.GetPrimaryKey().ToString()).SendAsync("CardPlayed", card.Value);

        if (currentHand.CurrentTrick.Cards.Count == 4)
        {
            var cards = currentHand.CurrentTrick.Cards;

            var maxOriginValue = cards.Where(i => i.Value.Type == currentHand.CurrentTrick.OriginalSuit).MaxBy(i => i.Value);
            var maxTrumpSuitValue = cards.Where(i => i.Value.Type == currentHand.TrumpSuit).DefaultIfEmpty().MaxBy(i => i.Value);

            var winnerId = maxOriginValue.Key;

            if (!maxTrumpSuitValue.Equals(default(KeyValuePair<long, ICard>)))
            {
                winnerId = maxTrumpSuitValue.Key;
            }

            if (winnerId == State.PlayerIds[0] || winnerId == State.PlayerIds[2])
                currentHand.FirstTeamTricks.Add(currentHand.CurrentTrick);
            else
                currentHand.SecondTeamTricks.Add(currentHand.CurrentTrick);

            currentHand.CurrentTrick = new Trick();
            currentHand.CurrentTrick.TurnId = winnerId;

            if (currentHand.FirstTeamTricks.Count == 7)
            {
                currentHand.Winner = Winner.FirstTeam;
                State.PlayedHands.Add(currentHand);

                var oldTrumpCallerIndex = State.PlayerIds.IndexOf(State.CurrentHand.TrumpCaller);
                var newTrumpCallerIndex = oldTrumpCallerIndex == 0 || oldTrumpCallerIndex == 2 ? oldTrumpCallerIndex : (oldTrumpCallerIndex + 1) % 4;

                State.CurrentHand = new();
                State.CurrentHand.TrumpCaller =State.PlayerIds[newTrumpCallerIndex];
                await StartGame();

            }
            else if (currentHand.SecondTeamTricks.Count == 7)
            {
                currentHand.Winner = Winner.SecondTeam;
                State.PlayedHands.Add(currentHand);

                var oldTrumpCallerIndex = State.PlayerIds.IndexOf(State.CurrentHand.TrumpCaller);
                var newTrumpCallerIndex = oldTrumpCallerIndex == 1 || oldTrumpCallerIndex == 3 ? oldTrumpCallerIndex : (oldTrumpCallerIndex + 1) % 4;

                State.CurrentHand = new();
                State.CurrentHand.TrumpCaller = State.PlayerIds[newTrumpCallerIndex];
                await StartGame();
            }

            if (State.PlayedHands.Count(i => i.Winner == Winner.FirstTeam) == 7)
            {
                State.IsFinished = true;
                // Message
            }
            else if (State.PlayedHands.Count(i => i.Winner == Winner.SecondTeam) == 7)
            {
                State.IsFinished = true;
                // Message
            }

        }
        else
        {
            var nextPlayerIndex = State.PlayerIds.IndexOf(currentHand.CurrentTrick.TurnId!.Value);
            nextPlayerIndex = (nextPlayerIndex + 1) % 4;
            currentHand.CurrentTrick.TurnId = State.PlayerIds[nextPlayerIndex];
        }
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
            State.CurrentHand.CurrentTrick.TurnId = State.PlayerIds[0];
            State.CurrentHand.TrumpCaller = State.PlayerIds[0];
            await StartGame();

            return JoinPlayerResult.GameStarted(this.GetPrimaryKey());
        }

        return JoinPlayerResult.Success(this.GetPrimaryKey());
    }

    private async Task StartGame()
    {
        var cards = GetCards().Chunk(13).ToArray();
        var i = 0;
        foreach (var item in State.PlayerIds)
        {
            this.State.CurrentHand.PlayerCards.Add(item, cards[i].ToList());
            i++;
        }
        await this.playerHub.Clients.User(State.CurrentHand.CurrentTrick.TurnId.Value.ToString()).SendAsync("ChooseTrumpSuit", cards[0].Take(5).ToArray());
    }

    public async Task ChooseTrumpSuit(CardTypes trumpSuit, IPlayer player)
    {
        if (player.GetPrimaryKeyLong() != this.State.CurrentHand.CurrentTrick.TurnId || player.GetPrimaryKeyLong() != this.State.CurrentHand.TrumpCaller || this.State.CurrentHand.TrumpSuit != null)
        {
            await this.playerHub.Clients.User(State.CurrentHand.TrumpCaller.ToString()).SendAsync("Error", "It is not Your Turn.");
            return;
        }

        this.State.CurrentHand.TrumpSuit = trumpSuit;
        await this.playerHub.Clients.Group(this.GetPrimaryKey().ToString()).SendAsync("TrumpSuit", trumpSuit);

        foreach (var (playerId, cards) in State.CurrentHand.PlayerCards)
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

    private static ICard[] GetCards()
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

