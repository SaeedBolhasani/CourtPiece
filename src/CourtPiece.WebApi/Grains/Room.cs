using CourtPiece.Common.Model;
using Microsoft.AspNetCore.SignalR;
using Orleans.Concurrency;
using Orleans.Providers;

[StorageProvider(ProviderName = StorageNames.DefaultEFStorageName)]
public class Room : Grain<RoomState>, IRoom
{
    private readonly IHubContext<RoomHub> hubContext;

    public Room(IHubContext<RoomHub> hubContext)
    {
        this.hubContext = hubContext;
    }

    public async Task Action(Immutable<ICard> card, IPlayer player)
    {
        long playerId = player.GetPrimaryKeyLong();

        if (State.IsFinished)
        {
            await SendMessageToPlayer(playerId, HubMethodNames.Error, "Game is over!");
            return;
        }

        var currentHand = this.State.CurrentHand;

        if (IsPlayerTurn() == false)
        {
            await SendMessageToPlayer(playerId, HubMethodNames.Error, "It is not Your Turn.");
            return;
        }

        if (HasPlayerThisCard() == false)
        {
            await SendMessageToPlayer(playerId, HubMethodNames.Error, "You have not this card.");
            return;
        }

        if (currentHand.CurrentTrick.OriginalSuit is null)
        {
            currentHand.CurrentTrick.OriginalSuit = card.Value.Type;
        }

        if (CanPlayThisCard() == false)
        {
            await SendMessageToPlayer(playerId, HubMethodNames.Error, "You can not play this card.");
            return;
        }

        currentHand.PlayerCards[playerId].Remove(card.Value);
        currentHand.CurrentTrick.Cards.Add(playerId, card.Value);

        await SendMessageToRoom(HubMethodNames.CardPlayed, card.Value);

        if (IsCurrentTrickCompleted())
        {
            var cards = currentHand.CurrentTrick.Cards;
            var winnerId = GetWinnerId(cards);

            if (State.FirstTeamPlayerIds.Contains(winnerId))
            {
                currentHand.FirstTeamTricks.Add(currentHand.CurrentTrick);
                await SendMessageToTeam(State.FirstTeamPlayerIds, HubMethodNames.TrickWinner, currentHand.FirstTeamTricks.Count);
            }
            else
            {
                currentHand.SecondTeamTricks.Add(currentHand.CurrentTrick);
                await SendMessageToTeam(State.SecondTeamPlayerIds, HubMethodNames.TrickWinner, currentHand.SecondTeamTricks.Count);
            }

            currentHand.CurrentTrick = new Trick();
            currentHand.CurrentTrick.TurnId = winnerId;

            var oldTrumpCallerIndex = State.PlayerIds.IndexOf(State.CurrentHand.TrumpCaller);
            var newTrumpCallerIndex = (oldTrumpCallerIndex + 1) % 4;

            bool handIsFinished = false;
            if (currentHand.FirstTeamTricks.Count == 7)
            {
                currentHand.Winner = Winner.FirstTeam;
                State.PlayedHands.Add(currentHand);

                newTrumpCallerIndex = oldTrumpCallerIndex == 0 || oldTrumpCallerIndex == 2 ? oldTrumpCallerIndex : newTrumpCallerIndex;
                handIsFinished = true;
                await SendMessageToRoom(HubMethodNames.HandWinner, "Team 1 won this hand!");

            }
            else if (currentHand.SecondTeamTricks.Count == 7)
            {
                currentHand.Winner = Winner.SecondTeam;
                State.PlayedHands.Add(currentHand);

                newTrumpCallerIndex = oldTrumpCallerIndex == 1 || oldTrumpCallerIndex == 3 ? oldTrumpCallerIndex : newTrumpCallerIndex;
                handIsFinished = true;
                await SendMessageToRoom(HubMethodNames.HandWinner, "Team 2 won this hand!");
            }

            if (State.PlayedHands.Count(i => i.Winner == Winner.FirstTeam) == 7)
            {
                State.IsFinished = true;
                await SendMessageToRoom(HubMethodNames.GameWinner, "Team 1 won this game!");
                return;
            }
            else if (State.PlayedHands.Count(i => i.Winner == Winner.SecondTeam) == 7)
            {
                State.IsFinished = true;
                await SendMessageToRoom(HubMethodNames.GameWinner, "Team 2 won this game!");
                return;
            }

            if (handIsFinished)
            {
                State.CurrentHand = new()
                {
                    TrumpCaller = State.PlayerIds[newTrumpCallerIndex],
                    CurrentTrick = new()
                    {
                        TurnId = State.PlayerIds[newTrumpCallerIndex]
                    }
                };
                await StartGame();
            }
        }
        else
        {
            var nextPlayerIndex = State.PlayerIds.IndexOf(currentHand.CurrentTrick.TurnId!.Value);
            nextPlayerIndex = (nextPlayerIndex + 1) % 4;
            currentHand.CurrentTrick.TurnId = State.PlayerIds[nextPlayerIndex];
        }

        await SendMessageToPlayer(State.CurrentHand.CurrentTrick.TurnId!.Value, HubMethodNames.ItIsYourTurn);


        bool IsPlayerTurn()
        {
            return playerId == currentHand.CurrentTrick.TurnId && currentHand.TrumpSuit != null;
        }

        bool HasPlayerThisCard()
        {
            return currentHand.PlayerCards[playerId].Contains(card.Value);
        }

        bool CanPlayThisCard()
        {
            return currentHand.CurrentTrick.OriginalSuit == card.Value.Type || currentHand.PlayerCards[playerId].Exists(i => i.Type == currentHand.CurrentTrick.OriginalSuit) == false;
        }

        bool IsCurrentTrickCompleted()
        {
            return currentHand.CurrentTrick.Cards.Count == 4;
        }

        long GetWinnerId(Dictionary<long, ICard> cards)
        {
            var maxOriginValue = cards.Where(i => i.Value.Type == currentHand.CurrentTrick.OriginalSuit).MaxBy(i => i.Value);
            var maxTrumpSuitValue = cards.Where(i => i.Value.Type == currentHand.TrumpSuit).DefaultIfEmpty().MaxBy(i => i.Value);

            var winnerId = maxOriginValue.Key;

            if (!maxTrumpSuitValue.Equals(default(KeyValuePair<long, ICard>)))
            {
                winnerId = maxTrumpSuitValue.Key;
            }

            return winnerId;
        }
    }



    public async Task<JoinPlayerResult> JoinPlayer(IPlayer player)
    {
        if (State.PlayerIds.Count >= 4)
        {
            return JoinPlayerResult.Error("Room is full.", this.GetPrimaryKey());
        }

        var playerId = player.GetPrimaryKeyLong();
        if (State.PlayerIds.Contains(playerId))
        {
            return JoinPlayerResult.Error("User has been joined already!", this.GetPrimaryKey());
        }

        var username = await player.GetFullName();
        await SendMessageToRoom(HubMethodNames.Room, "User Joined " + username);

        State.PlayerIds.Add(playerId);
        await SendMessageToPlayer(playerId, HubMethodNames.YouJoined, this.GetPrimaryKey());


        if (this.State.PlayerIds.Count == 4)
        {
            State.CurrentHand.CurrentTrick.TurnId = State.PlayerIds[0];
            State.CurrentHand.TrumpCaller = State.PlayerIds[0];

            State.FirstTeamPlayerIds.Add(State.PlayerIds[0]);
            State.FirstTeamPlayerIds.Add(State.PlayerIds[2]);

            State.SecondTeamPlayerIds.Add(State.PlayerIds[1]);
            State.SecondTeamPlayerIds.Add(State.PlayerIds[3]);

            await StartGame();

            return JoinPlayerResult.GameStarted(this.GetPrimaryKey());
        }

        return JoinPlayerResult.Success(this.GetPrimaryKey());
    }


    private async Task StartGame()
    {
        var cards = ServiceProvider.GetRequiredService<ICardProvider>().GetCards().Chunk(13).ToArray();
        var i = 0;
        foreach (var item in State.PlayerIds)
        {
            State.CurrentHand.PlayerCards.Add(item, cards[i].ToList());
            i++;
        }
        await SendMessageToPlayer(State.CurrentHand.TrumpCaller, HubMethodNames.ChooseTrumpSuit, cards[0].Take(5).ToArray());
    }

    public async Task ChooseTrumpSuit(CardTypes trumpSuit, IPlayer player)
    {
        if (player.GetPrimaryKeyLong() != this.State.CurrentHand.CurrentTrick.TurnId || player.GetPrimaryKeyLong() != this.State.CurrentHand.TrumpCaller || this.State.CurrentHand.TrumpSuit != null)
        {
            await SendMessageToPlayer(State.CurrentHand.TrumpCaller, HubMethodNames.Error, "It is not Your Turn.");
            return;
        }

        this.State.CurrentHand.TrumpSuit = trumpSuit;
        await SendMessageToRoom(HubMethodNames.TrumpSuit, trumpSuit);

        foreach (var (playerId, cards) in State.CurrentHand.PlayerCards)
        {
            await SendMessageToPlayer(playerId, HubMethodNames.Cards, cards);
        }
    }
    private async Task SendMessageToPlayer<T>(long playerId, string methodName, T message)
    {
        await this.hubContext.Clients.User(playerId.ToString()).SendAsync(methodName, message);
    }

    private async Task SendMessageToPlayer(long playerId, string methodName)
    {
        await this.hubContext.Clients.User(playerId.ToString()).SendAsync(methodName, null);
    }

    private async Task SendMessageToTeam<T>(IEnumerable<long> playerIds, string methodName, T message)
    {
        foreach (var playerId in playerIds)
        {
            await SendMessageToPlayer(playerId, methodName, message);
        }
    }

    private async Task SendMessageToRoom<T>(string methodName, T message)
    {
        await this.hubContext.Clients.Group(this.GetPrimaryKey().ToString()).SendAsync(methodName, message);
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await ReadStateAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await this.WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }


}

