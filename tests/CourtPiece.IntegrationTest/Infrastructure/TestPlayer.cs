using CourtPiece.Common.Model;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static AuthService;

namespace CourtPiece.IntegrationTest.Infrastructure
{

    public class TestPlayer
    {
        private const string Password = "Ab@123456";

        private readonly HttpClient httpClient;
        private readonly TestingWebAppFactory<Program> testingWebAppFactory;

        private readonly BlockingCollection<List<Card>> cardSemaphore = new(1);
        private readonly BlockingCollection<CardTypes> trumpSuitSemaphore = new(1);
        private readonly BlockingCollection<Card> playCardSemaphore = new(4);
        private readonly BlockingCollection<int> trickWinnerSemaphore = new(1);
        private readonly BlockingCollection<string> handWinnerCardSemaphore = new(1);
        private readonly BlockingCollection<string> gameWinnerCardSemaphore = new(1);
        private readonly BlockingCollection<Guid> roomIdBlockingCollection = new(1);

        private readonly TimeSpan maxWaitTime = TimeSpan.FromSeconds(10);

        private HubConnection connection;

        public List<Card> Cards { get; private set; }
        public string Error { get; private set; }
        public Guid? RoomId { get; private set; }
        public string Message { get; private set; }
        public CardTypes? TrumpSuit { get; private set; }
        public string GameWinnerMessage { get; private set; }
        public string HandWinnerMessage { get; private set; }
        public int? TrickWinnerCount { get; private set; }
        public string Name { get; private set; }

        public TestPlayer(TestingWebAppFactory<Program> testingWebAppFactory)
        {
            this.httpClient = testingWebAppFactory.CreateClient();
            this.testingWebAppFactory = testingWebAppFactory;
        }

        public async Task Create(string name)
        {
            Name = name;
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

            connection.On<List<Card>>(HubMethodNames.ChooseTrumpSuit, cardSemaphore.Add);
            connection.On<List<Card>>(HubMethodNames.Cards, cardSemaphore.Add);
            connection.On<Card>(HubMethodNames.CardPlayed, playCardSemaphore.Add);
            connection.On<CardTypes>(HubMethodNames.TrumpSuit, trumpSuitSemaphore.Add);
            connection.On<Guid>(HubMethodNames.YouJoined, roomIdBlockingCollection.Add);
            connection.On<int>(HubMethodNames.TrickWinner, trickWinnerSemaphore.Add);
            connection.On<string>(HubMethodNames.GameWinner, gameWinnerCardSemaphore.Add);
            connection.On<string>(HubMethodNames.HandWinner, handWinnerCardSemaphore.Add);

            connection.On<string>(HubMethodNames.Error, error =>
            {
                Error = error;
            });

            connection.On<string>(HubMethodNames.Room, message =>
            {
                Message = message;
            });


            connection.Closed += Connection_Closed;

            await connection.StartAsync();

        }

        private Task Connection_Closed(Exception? arg)
        {
            if (arg is not null)
                throw arg;
            return Task.CompletedTask;
        }

        //public async Task JoinRandomRoom()
        //{
        //    await connection.SendAsync("joinToRandomRoom");
        //}

        public async Task JoinRandomRoomAndWait()
        {
            await connection.SendAsync("joinToRandomRoom");
            RoomId = roomIdBlockingCollection.Take(CreateCancellationToken());
        }

        public async Task ChooseTrumpSuit(CardTypes type)
        {
            await connection.SendAsync("chooseTrumpSuit", type, RoomId.Value);
        }

        public void WaitForTrumpSuit()
        {
            TrumpSuit = trumpSuitSemaphore.Take(CreateCancellationToken());
        }

        public void WaitForTrumpCallerCards()
        {
            WaitForCards();
        }

        public void WaitForCards()
        {
            Cards = cardSemaphore.Take(CreateCancellationToken());
        }

        public async Task PlayAndWait(Card card)
        {
            await connection.SendAsync("action", card, RoomId.Value);

            var playedCard = playCardSemaphore.Take(CreateCancellationToken());
            while (!Cards.Contains(playedCard))
            {
                playedCard = playCardSemaphore.Take(CreateCancellationToken());
            }
            Cards.Remove(playedCard);
        }

        public void WaitForTrickWinnerAnnounce()
        {
            TrickWinnerCount = trickWinnerSemaphore.Take(CreateCancellationToken());
        }

        public void WaitForHandWinnerAnnounce()
        {
            HandWinnerMessage = handWinnerCardSemaphore.Take(CreateCancellationToken());
        }

        public void WaitForGameWinnerAnnounce()
        {
            GameWinnerMessage = gameWinnerCardSemaphore.Take(CreateCancellationToken());
        }

        private CancellationToken CreateCancellationToken()
        {
            return new CancellationTokenSource(maxWaitTime).Token;
        }
    }
}