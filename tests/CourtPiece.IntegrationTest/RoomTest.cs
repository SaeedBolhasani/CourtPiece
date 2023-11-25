using CourtPiece.Common.Model;
using CourtPiece.IntegrationTest.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Streams;
using static CourtPiece.Common.Model.Card;
namespace CourtPiece.IntegrationTest
{
    public partial class RoomTest : IClassFixture<TestingWebAppFactory<Program>>
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
            //var roomId = Guid.NewGuid();

            //var semaphore = new SemaphoreSlim(0);

            //// Create and join 4 players
            //var p1 = new TestPlayer(testingWebAppFactory, semaphore);
            //await p1.Create("player1");
            //var t1 = Task.Run(() => p1.Join(roomId));

            //var p2 = new TestPlayer(testingWebAppFactory, semaphore);
            //await p2.Create("player2");
            //var t2 = Task.Run(() => p2.Join(roomId));

            //var p3 = new TestPlayer(testingWebAppFactory, semaphore);
            //await p3.Create("player3");
            //var t3 = Task.Run(() => p3.Join(roomId));

            //var p4 = new TestPlayer(testingWebAppFactory, semaphore);
            //await p4.Create("player4");
            //var t4 = Task.Run(() => p4.Join(roomId));

            //await t1;
            //Task.WaitAll(t2, t3, t4);
            ////semaphore.WaitOne();
        }

        [Fact]
        public async void Test_JoinRandomRoom()
        {
            const CardTypes TrumpSuit = CardTypes.Hearts;
            var turnBuilder = new TrunBuilder
            {
                //player1       player2         player3     player4    next winner index
                { HeartAce,     Heart4,         Heart2,     Heart3,       0 },
                { Spade9,       SpadeQueen,     Spade10,    Spade2,       1 },
                { SpadeKing,    SpadeAce,       Spade5,     SpadeJack,    1 },
                { DiamondKing,  Diamond3,       Diamond2,   DiamondJack,  0 },
                { DiamondAce,   DiamondQueen,   Diamond6,   Diamond7,     0 },
                { ClubJack,     Club7,          Club4,      ClubAce,      3 },
                { Club2,        Spade8,         Club3,      ClubKing,     3 },
                { Club5,        HeartJack,      Club8,      Club6,        1 },
                { Heart7,       HeartKing,      Heart5,     Club10,       1 },
                { Heart9,       HeartQueen,     Heart10,    Spade3,       1 },
                { Club5,        HeartQueen,     Club8,      Spade3,       1 },
                { Club5,        HeartQueen,     Club8,      Spade3,       1 },
                { Club5,        HeartQueen,     Club8,      Spade3,       1 },
            };


            var cards = turnBuilder.GetAllCardsOrderedByPlayerIndex().ToArray();
            this.mockCardProvider.Setup(i => i.GetCards()).Returns(cards);

            var allPlayers = new List<TestPlayer>(4);
            // Create and join 4 players
            var p1 = new TestPlayer(testingWebAppFactory);
            await p1.Create("player1");
            var t1 = Task.Run(p1.JoinRandomRoomAndWait);
            allPlayers.Add(p1);

            var p2 = new TestPlayer(testingWebAppFactory);
            await p2.Create("player2");
            await p2.JoinRandomRoom();
            allPlayers.Add(p2);

            var p3 = new TestPlayer(testingWebAppFactory);
            await p3.Create("player3");
            await p3.JoinRandomRoom();
            allPlayers.Add(p3);

            var p4 = new TestPlayer(testingWebAppFactory);
            await p4.Create("player4");
            await p4.JoinRandomRoom();
            allPlayers.Add(p4);

            await t1;

            p1.Cards.Count.Should().Be(5);

            p2.RoomId.Should().Be(p1.RoomId);
            p3.RoomId.Should().Be(p1.RoomId);
            p4.RoomId.Should().Be(p1.RoomId);


            await p1.ChooseTrumpSuit(TrumpSuit);

            allPlayers.ForEach(i => i.WaitForTrumpSuit());
            allPlayers.Should().AllSatisfy(i => i.TrumpSuit.Should().Be(TrumpSuit));

            allPlayers.ForEach(i => i.WaitForCards());

            var winnerIndex = 0;
            foreach (var (turnCards, nextWinnerIndex) in turnBuilder.Take(10))
            {
                await allPlayers[winnerIndex].PlayAndWait(turnCards[winnerIndex]);
                await allPlayers[(winnerIndex + 1) % 4].PlayAndWait(turnCards[(winnerIndex + 1) % 4]);
                await allPlayers[(winnerIndex + 2) % 4].PlayAndWait(turnCards[(winnerIndex + 2) % 4]);
                await allPlayers[(winnerIndex + 3) % 4].PlayAndWait(turnCards[(winnerIndex + 3) % 4]);
                winnerIndex = nextWinnerIndex;

                
            }

        }
    }


}