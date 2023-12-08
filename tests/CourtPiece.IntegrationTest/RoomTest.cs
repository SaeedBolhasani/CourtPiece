using CourtPiece.Common.Model;
using CourtPiece.IntegrationTest.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
        public async void Join4PlayerToOneRandomRoom_Play7Hands_WinnerIsTeam2()
        {
            const CardTypes TrumpSuit = CardTypes.Hearts;
            var turnBuilder = new GameBuilder
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
            await p1.JoinRandomRoomAndWait();
            allPlayers.Add(p1);

            var p2 = new TestPlayer(testingWebAppFactory);
            await p2.Create("player2");
            await p2.JoinRandomRoomAndWait();
            allPlayers.Add(p2);

            var p3 = new TestPlayer(testingWebAppFactory);
            await p3.Create("player3");
            await p3.JoinRandomRoomAndWait();
            allPlayers.Add(p3);

            var p4 = new TestPlayer(testingWebAppFactory);
            await p4.Create("player4");
            await p4.JoinRandomRoomAndWait();
            allPlayers.Add(p4);

            p2.RoomId.Should().Be(p1.RoomId);
            p3.RoomId.Should().Be(p1.RoomId);
            p4.RoomId.Should().Be(p1.RoomId);

            foreach (var index in Enumerable.Range(0, 7))
            {
                await PlayHand(TrumpSuit, turnBuilder, allPlayers, index == 0 ? 0 : 1);

                allPlayers.ForEach(i => i.WaitForHandWinnerAnnounce());
                allPlayers.Should().AllSatisfy(i => i.HandWinnerMessage.Should().Be("Team 2 won this hand!"));
            }

            allPlayers.ForEach(i => i.WaitForGameWinnerAnnounce());
            allPlayers.Should().AllSatisfy(i => i.GameWinnerMessage.Should().Be("Team 2 won this game!"));

        }

        private static async Task PlayHand(CardTypes TrumpSuit, GameBuilder turnBuilder, List<TestPlayer> allPlayers, int trumpCallerIndex)
        {
            allPlayers[trumpCallerIndex].WaitForTrumpCallerCards();
            allPlayers[trumpCallerIndex].Cards.Count.Should().Be(5);

            await allPlayers[trumpCallerIndex].ChooseTrumpSuit(TrumpSuit);

            allPlayers.ForEach(i => i.WaitForTrumpSuit());
            allPlayers.Should().AllSatisfy(i => i.TrumpSuit.Should().Be(TrumpSuit));

            allPlayers.ForEach(i => i.WaitForCards());

            var winnerIndex = trumpCallerIndex;
            var firstTeamWonTrick = 0;
            var secondTeamWonTrick = 0;
            foreach (var (turnCards, nextWinnerIndex) in turnBuilder)
            {
                await allPlayers[winnerIndex].PlayAndWait(turnCards[winnerIndex]);
                await allPlayers[(winnerIndex + 1) % 4].PlayAndWait(turnCards[(winnerIndex + 1) % 4]);
                await allPlayers[(winnerIndex + 2) % 4].PlayAndWait(turnCards[(winnerIndex + 2) % 4]);
                await allPlayers[(winnerIndex + 3) % 4].PlayAndWait(turnCards[(winnerIndex + 3) % 4]);

                winnerIndex = nextWinnerIndex;

                if (winnerIndex is 0 or 2)
                {
                    firstTeamWonTrick++;
                    allPlayers[0].WaitForTrickWinnerAnnounce();
                    allPlayers[0].TrickWinnerCount.Should().Be(firstTeamWonTrick);

                    allPlayers[2].WaitForTrickWinnerAnnounce();
                    allPlayers[2].TrickWinnerCount.Should().Be(firstTeamWonTrick);
                }
                else
                {
                    secondTeamWonTrick++;
                    allPlayers[1].WaitForTrickWinnerAnnounce();
                    allPlayers[1].TrickWinnerCount.Should().Be(secondTeamWonTrick);

                    allPlayers[3].WaitForTrickWinnerAnnounce();
                    allPlayers[3].TrickWinnerCount.Should().Be(secondTeamWonTrick);
                }

                if (firstTeamWonTrick == 7 || secondTeamWonTrick == 7)
                    break;
            }
        }
    }


}