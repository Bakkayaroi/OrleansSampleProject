using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Server;
using Server.Grains.Interfaces;

namespace Client
{
    public class Program
    {
        private static IPlayerGrain PlayerGrain;

        private static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddFilter("Orleans", LogLevel.Warning);
                    logging.AddFilter("Microsoft", LogLevel.Warning);
                })
                .UseOrleansClient(clientBuilder =>
                {
                    clientBuilder.UseLocalhostClustering();
                    clientBuilder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "GameServer";
                    });
                })
                .Build();

            await host.StartAsync();
            var client = host.Services.GetRequiredService<IClusterClient>();
            PlayerGrain = await GetValidPlayerGrain(client);

            var playerClient = new PlayerClient(PlayerGrain);
            var observer = client.CreateObjectReference<IPlayerClient>(playerClient);
            await PlayerGrain.Subscribe(observer);

            await StartGame(playerClient);

            await PlayerGrain.UnSubscribe(observer);
            await host.StopAsync();
        }

        private static async Task StartGame(PlayerClient playerClient)
        {
            await playerClient.ShowScore();

            await playerClient.StartMatchmaking();

            await WaitForCondition(() => playerClient.LobbyGrain != null, "Ожидание лобби...");
            
            var number = GetValidNumber();

            await playerClient.SendNumber(number);

            await WaitForCondition(() => playerClient.ResultModel != null, "Ожидание игоков...");
            
            playerClient.ShowResult(number);

            Console.WriteLine("Продолжить ? (y/n)");
            var answer = Console.ReadLine();
            if (string.Equals(answer, "y"))
            {
                await RestartGame(playerClient);
            }
        }

        private static async Task WaitForCondition(Func<bool> condition, string message)
        {
            int spinnerIndex = 0;
            char[] spinner = { '|', '/', '-', '\\' };

            Console.Write(message);

            while (!condition())
            {
                Console.Write($"\b{spinner[spinnerIndex]}");
                spinnerIndex = (spinnerIndex + 1) % spinner.Length;
                await Task.Delay(100);
            }
        }

        private static async Task RestartGame(PlayerClient playerClient)
        {
            playerClient.Dispose();
            await StartGame(playerClient);
        }

        private static int GetValidNumber()
        {
            string? input;
            int value;

            Console.WriteLine();
            Console.WriteLine("Введите число от 0 до 100: ");
            input = Console.ReadLine();

            while (!int.TryParse(input, out value) || value < 0 || value > 100)
            {
                Console.WriteLine("Неверный ввод. Пожалуйста, попробуйте снова.");
                Console.Write("Введите число от 0 до 100: ");
                input = Console.ReadLine();
            }

            return value;
        }

        private static async Task<IPlayerGrain> GetValidPlayerGrain(IClusterClient client)
        {
            var playerId = GetValidPlayerId();
            var playerGrain = client.GetGrain<IPlayerGrain>(playerId);
            var hasLobby = await playerGrain.HasLobby();

            while (hasLobby)
            {
                Console.WriteLine("Пользователь с таким логином уже в игре!.");

                playerId = GetValidPlayerId();
                playerGrain = client.GetGrain<IPlayerGrain>(playerId);
                hasLobby = await playerGrain.HasLobby();
            }

            return playerGrain;
        }

        private static string GetValidPlayerId()
        {
            Console.WriteLine("Введите логин:");
            var playerId = Console.ReadLine();

            while (string.IsNullOrEmpty(playerId) || !Regex.IsMatch(playerId, "^[a-zA-Z0-9]+$"))
            {
                Console.WriteLine("Логин должен состоять только из букв латинского алфавита и цифр.");
                Console.Write("Введите логин:");

                playerId = Console.ReadLine();
            }

            return playerId;
        }
    }
}