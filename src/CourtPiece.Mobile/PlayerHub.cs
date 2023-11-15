using Microsoft.AspNetCore.SignalR.Client;

namespace CourtPiece.Mobile
{
    public class PlayerHub
    {

        public async Task<HubConnection> Connect(string token)
        {
            var connectionBuilder = new HubConnectionBuilder()
               .WithUrl("http://localhost:5182/ChatHub", i =>
               {
                   i.Headers.Add("Authorization", "Bearer " + token);                   
               })               ;
            
            var c = connectionBuilder.Build();
           
            await c.StartAsync();
            return c;
        }
    }
}
