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

        public event EventHandler<string> OnMessageReceived;

        private bool chooseTrumpSuit = false;

        private Guid roomId;
        public PlayerService(HttpClient httpClient)
        {            
            this.httpClient = httpClient;
            httpClient.BaseAddress = new Uri("http://localhost:5182/");
        }
        public event EventHandler<Card[]> OnCardReceived;
        public event EventHandler OnMyTurn;
        internal async Task Join(string username)
        {
            //if (this.hubConnection != null) return;

            var token = await httpClient.PostAsJsonAsync("api/Authentication/login", new
            {
                UserName = username,
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

            hubConnection.On<Card>("CardPlayed", i =>
            {
                this.OnMessageReceived?.Invoke(this, i.ToString());
                Console.WriteLine(i);
            });

            
            hubConnection.On<string>("Room", i =>
            {
                this.OnMessageReceived?.Invoke(this, i);
                Console.WriteLine(i);
            });
            hubConnection.On<Guid>("YouJoined", i =>
            {
                this.roomId = i;
            });

            hubConnection.On<string>("Error", i =>
            {
                this.OnMessageReceived?.Invoke(this, i);

                Console.WriteLine(i);
            });

            hubConnection.On<Card[]>("ChooseTrumpSuit", i =>
            {
                this.cards = i;
                OnCardReceived?.Invoke(this, i);
                chooseTrumpSuit = true;
            });

            hubConnection.On<Card[]>("Cards", i =>
            {
                this.cards = i;
                this.chooseTrumpSuit = false;
                OnCardReceived?.Invoke(this, i);
                Console.WriteLine(i);
            });

            hubConnection.On<Card[]>("TrumpSuit", i =>
            {
                Console.WriteLine(i);
            });

            hubConnection.On<object>("ItIsYourTurn", i =>
            {
                OnMyTurn?.Invoke(this, new EventArgs());
            });

            hubConnection.On<string>("HandWinner", i =>
            {
                OnCardReceived?.Invoke(this, Array.Empty<Card>());
                OnMessageReceived?.Invoke(this, i);
            });


            hubConnection.On<string>("GameWinner", i =>
            {
                OnCardReceived?.Invoke(this, Array.Empty<Card>());
                OnMessageReceived?.Invoke(this, i);
            });


            hubConnection.Closed += HubConnection_Closed;
            await hubConnection.SendAsync("joinToRandomRoom");
        }

        private Task HubConnection_Closed(Exception arg)
        {
            return Task.CompletedTask;
        }

        public async Task Action(Card card)
        {
            if (chooseTrumpSuit)
                await hubConnection.SendAsync("ChooseTrumpSuit", card.Type, roomId.ToString());
            else
                await hubConnection.SendAsync("action", card, roomId.ToString());


        }
    }
}
