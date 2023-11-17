using CourtPiece.Common.Model;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace CourtPiece.Mobile
{
    public class PlayerService
    {
        private readonly HttpClient httpClient;
        private Card[] cards;
        private HubConnection hubConnection;

        public PlayerService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            httpClient.BaseAddress = new Uri("http://localhost:5182/");
        }
        public event EventHandler<Card[]> OnCardReceived;
        internal async Task Join(int id, Guid roomId)
        {
            //if (this.hubConnection != null) return;

            var token = await httpClient.PostAsJsonAsync("api/Authentication/login", new
            {
                UserName = "test" + id,
                Password = "Ab@123456"
            });
            token.EnsureSuccessStatusCode();

            var t = await token.Content.ReadAsStringAsync();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t);

        

            hubConnection = await new PlayerHub().Connect(t);
           

            hubConnection.On<Card[]>("Game", i =>
            {
                this.cards = i;
                OnCardReceived?.Invoke(this, i);
               Console.WriteLine(i);
            });
            hubConnection.On<string>("Room", i =>
            {
                Console.WriteLine(i);
            });
            hubConnection.On<string>("PlayerJoined", i =>
            {
                Console.WriteLine(i);
            });

            hubConnection.On<object>("Error", i =>
            {
                Console.WriteLine(i);
            });


            hubConnection.Closed += HubConnection_Closed;
            await hubConnection.SendAsync("joinToRandomRoom");
            //var result = await httpClient.GetAsync($"api/player/join?roomId={roomId}");
        }

        private Task HubConnection_Closed(Exception arg)
        {
            return Task.CompletedTask;
        }

        public async Task Action(Card card,Guid roomId)
        {
            await hubConnection.SendAsync("action", card, roomId.ToString());

        }
    }
}
