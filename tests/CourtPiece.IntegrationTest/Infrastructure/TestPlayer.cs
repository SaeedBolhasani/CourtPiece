using CourtPiece.Common.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static AuthService;

namespace CourtPiece.IntegrationTest.Infrastructure
{

    public class TestPlayer
    {
        private readonly HttpClient httpClient;
        private readonly TestingWebAppFactory<Program> testingWebAppFactory;
        private HubConnection connection;
        private SemaphoreSlim semaphore;
        private const string Password = "Ab@123456";
        private readonly SemaphoreSlim trumpSuitMutex = new(0);

        public List<Card> Cards { get; private set; }
        public string Error { get; private set; }
        public Guid? RoomId { get; private set; }
        public string Message { get; private set; }
        public CardTypes? TrumpSuit { get; private set; }
        public string GameWinnerMessage { get; private set; }
        public string HandWinner { get; private set; }

        public TestPlayer(TestingWebAppFactory<Program> testingWebAppFactory, SemaphoreSlim semaphore)
        {
            this.httpClient = testingWebAppFactory.CreateClient();
            this.testingWebAppFactory = testingWebAppFactory;
            this.semaphore = semaphore;
        }

        public async Task Create(string name)
        {
            var result = await httpClient.PostAsJsonAsync("api/Authentication/registeration", new RegistrationModel
            {
                Email = $"{name}@{name}.com",
                FirstName = name,
                Username = name,
                LastName = name,
                Password = Password,
            });
            result.EnsureSuccessStatusCode();

            result = await httpClient.PostAsJsonAsync("api/Authentication/login", new LoginModel
            {
                Password = Password,
                Username = name
            });

            result.EnsureSuccessStatusCode();
            var token = await result.Content.ReadAsStringAsync();

            var authenticationHeader = new AuthenticationHeaderValue("Bearer", token);
            this.httpClient.DefaultRequestHeaders.Authorization = authenticationHeader;

            connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/ChatHub", i =>
            {
                i.Headers.Add("Authorization", authenticationHeader.ToString());
                i.HttpMessageHandlerFactory = _ => this.testingWebAppFactory.Server.CreateHandler();
            })
            .Build();

            connection.On<Card[]>(HubMethodNames.ChooseTrumpSuit, cards =>
            {
                Cards = cards.ToList();
                semaphore.Release();
            });

            connection.On<string>(HubMethodNames.Error, error =>
            {
                Error = error;
            });

            connection.On<Card>(HubMethodNames.CardPlayed, error =>
            {

            });

            connection.On<Guid>(HubMethodNames.YouJoined, roomId =>
            {
                RoomId = roomId;
            });

            connection.On<string>(HubMethodNames.Room, message =>
            {
                Message = message;
            });
            connection.On<CardTypes>(HubMethodNames.TrumpSuit, trumpSuit =>
            {
                TrumpSuit = trumpSuit;
                trumpSuitMutex.Release();
            });
            connection.On<string>(HubMethodNames.GameWinner, message =>
            {
                GameWinnerMessage = message;
            });
            connection.On<string>(HubMethodNames.HandWinner, message =>
            {
                HandWinner = message;
            });

            connection.On<List<Card>>(HubMethodNames.Cards, error =>
            {
                Cards = error;
            });

            await connection.StartAsync();

        }

        public async Task Join(Guid roomId)
        {
            await connection.SendAsync("join", roomId.ToString());
            semaphore.Wait();
        }

        public async Task JoinRandomRoom()
        {
            await connection.SendAsync("joinToRandomRoom");
        }

        public async Task JoinRandomRoomAndWait()
        {
            await connection.SendAsync("joinToRandomRoom");
            semaphore.Wait();
        }

        public async Task ChooseTrumpSuit(CardTypes type)
        {
            await connection.SendAsync("chooseTrumpSuit", type, RoomId.Value);
        }

        public void WaitForTrumpSuit()
        {
            if (TrumpSuit == null)
                trumpSuitMutex.Wait();
        }
    }
}