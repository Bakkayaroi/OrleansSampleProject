using Server.Grains.Interfaces;
using Server.Models;

namespace Server;

public class PlayerClient(IPlayerGrain playerGrain) : IPlayerClient
{
    public ILobbyGrain? LobbyGrain { get; private set; }
    public RoundResultModel? ResultModel { get; private set; }

    public async Task LobbyOpened(ILobbyGrain lobbyGrain)
    {
        LobbyGrain = lobbyGrain;
        var players = await LobbyGrain.GetPlayers();
        Console.Write($"\b ");
        Console.WriteLine();
        Console.WriteLine("Лобби найдено");
        Console.WriteLine("Игроки в лобби:");

        Console.Write("| ");
        for (var i = 0; i < players.Count; i++)
        {
            Console.Write($"Игрок{i + 1}: {players[i]}");
            Console.Write(" | ");
        }
        Console.WriteLine();
    }

    public Task LobbyClosed(RoundResultModel roundResultModel)
    {
        ResultModel = roundResultModel;
        return Task.CompletedTask;
    }

    public async Task ShowScore()
    {
        var score = await playerGrain.GetScore();
        Console.WriteLine($"Ваши очки:{score}");
    }

    public async Task SendNumber(int number)
    {
        await playerGrain.SendNumber(number);
    }

    public async Task StartMatchmaking()
    {
        await playerGrain.AddPlayerToQueue();
    }

    public void ShowResult(int guessNumber)
    {
        if (ResultModel == null)
        {
            throw new ArgumentNullException();
        }

        Console.Write($"\b ");
        Console.WriteLine();
        Console.WriteLine($"Победитель: {ResultModel.WinnerPlayer}!");
        Console.WriteLine($"Число победителя: {ResultModel.WinnerNumber}");
        Console.WriteLine($"Загаданное число: {ResultModel.TargetNumber}");
        Console.WriteLine();
        Console.WriteLine($"Ваше число: {guessNumber}");
    }

    public void Dispose()
    {
        ResultModel = null;
        LobbyGrain = null;
    }
}