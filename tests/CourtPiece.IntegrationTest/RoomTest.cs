using CourtPiece.Common.Model;
using CourtPiece.IntegrationTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Streams;

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
            var cards = Card.AllCards.OrderBy(i=>i.Type).ThenBy(i=>i.Value).ToArray();

            this.mockCardProvider.Setup(i => i.GetCards()).Returns(cards);

            var semaphore = new SemaphoreSlim(0);

            // Create and join 4 players
            var p1 = new TestPlayer(testingWebAppFactory, semaphore);
            await p1.Create("player1");
            var t1 = Task.Run(p1.JoinRandomRoom);

            var p2 = new TestPlayer(testingWebAppFactory, semaphore);
            await p2.Create("player2");
            var t2 = Task.Run(p2.JoinRandomRoom);

            var p3 = new TestPlayer(testingWebAppFactory, semaphore);
            await p3.Create("player3");
            var t3 = Task.Run(p3.JoinRandomRoom);

            var p4 = new TestPlayer(testingWebAppFactory, semaphore);
            await p4.Create("player4");
            var t4 = Task.Run(p4.JoinRandomRoom);

            await t1;
            Task.WaitAll(t2, t3, t4);
        }


        [Fact]
        public async void Test2()
        {
            var ss = testingWebAppFactory.Services.GetRequiredService<IClusterClient>();
            var streamProvider = ss.GetStreamProvider("test");
            await streamProvider.GetStream<string>("test").SubscribeAsync(async i =>
            {
                await Task.CompletedTask;
            });
            //await t6;

            var roomId = Guid.NewGuid();
            var semaphore = new SemaphoreSlim(0);
            var p1 = new TestPlayer(testingWebAppFactory, semaphore);
            await p1.Create("player1");
            var t1 = Task.Run(() => p1.Join(roomId));
            //await t1;

            var p3 = new TestPlayer(testingWebAppFactory, semaphore);
            await p3.Create("player3");
            var t3 = Task.Run(() => p3.Join(roomId));

            await Task.Delay(40000);

        }


    }


}