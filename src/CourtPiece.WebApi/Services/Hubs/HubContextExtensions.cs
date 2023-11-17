using Microsoft.AspNetCore.SignalR;

public static class HubContextExtensions
{
    public static async Task SendToUser(this IHubContext<RoomHub> hubContext, long user, object message)
    {
        await hubContext.Clients.User(user.ToString()).SendAsync("Game", message);
    }

    public static async Task SentToRoom(this IHubContext<RoomHub> hubContext, Guid roomId, object message)
    {
        await hubContext.Clients.Group(roomId.ToString()).SendAsync("Room", message);
    }

    public static async Task SentToRoomExcept(this IHubContext<RoomHub> hubContext, Guid roomId, object message, params long[] playerIds)
    {
        await hubContext.Clients.GroupExcept(roomId.ToString(), playerIds.Select(i => i.ToString())).SendAsync("Game", message);
    }
}
