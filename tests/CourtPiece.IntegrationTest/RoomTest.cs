using CourtPiece.Common.Model;
using CourtPiece.IntegrationTest.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Streams;
using static CourtPiece.Common.Model.Card;
namespace CourtPiece.IntegrationTest
{
    public class RoomTest : IClassFixture<TestingWebAppFactory<Program>>
    {
        private readonly TestingWebAppFactory<Program> testingWebAppFactory;
        private readonly Mock<ICardProvider> mockCardProvider;

        public RoomTest(TestingWebAppFactory<Program> testingWebAppFactory)
        {
            this.testingWebAppFactory = testingWebAppFactory;
            this.mockCardProvider = testingWebAppFactory.Services.GetRequiredService<Mock<ICardProvider>>();
        }

        [Fact]
        public async void Test()
        {
            var roomId = Guid.NewGuid();

            var semaphore = new SemaphoreSlim(0);

            // Create and join 4 players
            var p1 = new TestPlayer(testingWebAppFactory, semaphore);
            await p1.Create("player1");
            var t1 = Task.Run(() => p1.Join(roomId));

            var p2 = new TestPlayer(testingWebAppFactory, semaphore);
            await p2.Create("player2");
            var t2 = Task.Run(() => p2.Join(roomId));

            var p3 = new TestPlayer(testingWebAppFactory, semaphore);
            await p3.Create("player3");
            var t3 = Task.Run(() => p3.Join(roomId));

            var p4 = new TestPlayer(testingWebAppFactory, semaphore);
            await p4.Create("player4");
            var t4 = Task.Run(() => p4.Join(roomId));

            await t1;
            Task.WaitAll(t2, t3, t4);
            //semaphore.WaitOne();
        }



        [Fact]
        public async void Test_JoinRandomRoom()
        {
            var r = new Random();
            var c = Card.AllCards.OrderBy(i => r.Next()).ToArray();

            var s = string.Join(",\t", c.Select(i => i.ToString()));


            //trump suit is Diamond
            var cards = new[]
            {
                                        HeartAce,   HeartKing,      Heart10,                          //first player cards
                                        Heart2,     Heart3,         HeartJack,     Heart5,    Heart4,  Spade10 ,
                                        Heart5,     Heart6,         Diamond2,
                                        Heart4,     HeartQueen,     Diamond4,    Heart5, Heart6,
            };
            var turnIndexes = new[] { 0, 0, 0, };

            this.mockCardProvider.Setup(i => i.GetCards()).Returns(cards);

            var semaphore = new SemaphoreSlim(0);

            // Create and join 4 players
            var p1 = new TestPlayer(testingWebAppFactory, semaphore);
            await p1.Create("player1");
            var t1 = Task.Run(p1.JoinRandomRoomAndWait);

            var p2 = new TestPlayer(testingWebAppFactory, semaphore);
            await p2.Create("player2");
            await p2.JoinRandomRoom();

            var p3 = new TestPlayer(testingWebAppFactory, semaphore);
            await p3.Create("player3");
            await p3.JoinRandomRoom();

            var p4 = new TestPlayer(testingWebAppFactory, semaphore);
            await p4.Create("player4");
            await p4.JoinRandomRoom();

            await t1;

            p1.Cards.Count.Should().Be(5);

            p2.RoomId.Should().Be(p1.RoomId);
            p3.RoomId.Should().Be(p1.RoomId);
            p4.RoomId.Should().Be(p1.RoomId);


            await p1.ChooseTrumpSuit(cards[0].Type);

            p1.WaitForTrumpSuit();
            p2.WaitForTrumpSuit();
            p3.WaitForTrumpSuit();
            p4.WaitForTrumpSuit();

            p1.TrumpSuit.Should().Be(cards[0].Type);
            p2.TrumpSuit.Should().Be(cards[0].Type);
            p3.TrumpSuit.Should().Be(cards[0].Type);
            p4.TrumpSuit.Should().Be(cards[0].Type);

            // Task.WaitAll(t2, t3, t4);
        }
    }


}