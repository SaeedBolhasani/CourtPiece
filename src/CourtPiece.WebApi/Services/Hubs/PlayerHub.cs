using CourtPiece.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans.Concurrency;
using System.Security.Claims;

[Authorize]
public class PlayerHub : Hub
{
    public async Task Join(Guid roomId, IGrainFactory grainFactory)
    {
        int userId = GetUserId();
        var grain = grainFactory.GetGrain<IPlayer>(userId);
        var room = grainFactory.GetGrain<IRoom>(roomId);
        await grain.Join(room);
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, roomId.ToString());
        var c = this.Clients.Group(roomId.ToString());
        await c.SendAsync("PlayerJoined", userId);
    }


    public async Task Action(Card card, Guid roomId, IGrainFactory grainFactory)
    {
        var userId = GetUserId();
        var player = grainFactory.GetGrain<IPlayer>(userId);
        var room = grainFactory.GetGrain<IRoom>(roomId);
        await room.Action(new Immutable<ICard>(card), player);
    }
    private int GetUserId()
    {
        return int.Parse(((ClaimsIdentity)Context!.User!.Identity!).FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
