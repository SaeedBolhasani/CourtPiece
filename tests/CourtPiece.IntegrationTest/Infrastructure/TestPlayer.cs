﻿using CourtPiece.Common.Model;
using Microsoft.AspNetCore.SignalR.Client;
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

        public Card[] Cards { get; private set; }
        public string Error { get; private set; }

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
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/ChatHub", i =>
            {
                i.Headers.Add("Authorization", "Bearer " + token);
                i.HttpMessageHandlerFactory = _ => this.testingWebAppFactory.Server.CreateHandler();
            })
            .Build();

            connection.On<List<Card>>(HubMethodNames.ChooseTrumpSuit, cards =>
            {
                this.Cards = cards.OrderBy(i => i.Type).ThenBy(i => i.Value).ToArray();
                semaphore.Release();
            });

            connection.On<string>(HubMethodNames.Error, error =>
            {
               Error = error;
            });

            connection.On<Card>(HubMethodNames.CardPlayed, error =>
            {
                Error = error;
            });

            connection.On<string>(HubMethodNames.YouJoined, error =>
            {
                Error = error;
            });

            connection.On<string>(HubMethodNames.Room, error =>
            {
                Error = error;
            });
            connection.On<string>(HubMethodNames.TrumpSuit, error =>
            {
                Error = error;
            });
            connection.On<string>(HubMethodNames.GameWinner, error =>
            {
                Error = error;
            });
            connection.On<string>(HubMethodNames.HandWinner, error =>
            {
                Error = error;
            });

            connection.On<string>(HubMethodNames.Cards, error =>
            {
                Error = error;
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
            semaphore.Wait();
        }
    }   
}