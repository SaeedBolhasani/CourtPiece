using CourtPiece.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using Orleans.Concurrency;
using System.Security.Claims;

[Authorize]
public class RoomHub : Hub
{
    private readonly ILogger<RoomHub> logger;

    public RoomHub(ILogger<RoomHub> logger)
    {
        this.logger = logger;
    }
    
    public async Task Join(Guid roomId, IGrainFactory grainFactory)
    {
        try
        {
            int userId = GetUserId();
            var roomManager = GetRoomManager(grainFactory);
            var result = await roomManager.JoinToRoom(roomId, grainFactory.GetGrain<IPlayer>(userId));

            if (result.IsSuccess == false)
            {
                await this.Clients.User(userId.ToString()).SendAsync("Error", result.ErrorMessage, default);
                Context.Abort();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in Class: {0}, Method: {1}. User Id:{2}. Room Id: {3}", nameof(RoomHub), nameof(Join), GetUserId(), roomId);
            throw;
        }

    }

    public async Task JoinToRandomRoom(IGrainFactory grainFactory)
    {
        try
        {
            int userId = GetUserId();
            var roomManager = GetRoomManager(grainFactory);
            var result = await roomManager.JoinToRandomRoom(grainFactory.GetGrain<IPlayer>(userId));

            if (result.IsSuccess == false)
            {
                await this.Clients.User(userId.ToString()).SendAsync("Error", result.ErrorMessage, default);
                Context.Abort();
            }
            //await this.Clients.User(userId.ToString()).SendAsync("UserJoinedSuccessfully", retus, default);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in Class: {0}, Method: {1}. User Id:{2}.", nameof(RoomHub), nameof(Join), GetUserId());
            throw;
        }
    }

    public async Task Action(Card card, Guid roomId, IGrainFactory grainFactory)
    {
        var userId = GetUserId();
        var player = grainFactory.GetGrain<IPlayer>(userId);
        var room = grainFactory.GetGrain<IRoom>(roomId);
        await room.Action(new Immutable<ICard>(card), player);
    }

    private static IRoomManager GetRoomManager(IGrainFactory grainFactory)
    {
        return grainFactory.GetGrain<IRoomManager>(0);
    }

    private int GetUserId()
    {
        return int.Parse(((ClaimsIdentity)Context!.User!.Identity!).FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            logger.LogError(exception, "An error occurred in Class: {0}, Method: {1}. User Id:{2}.", nameof(RoomHub), nameof(OnDisconnectedAsync), GetUserId());

        return base.OnDisconnectedAsync(exception);
    }
}
