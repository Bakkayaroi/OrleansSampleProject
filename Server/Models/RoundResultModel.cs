namespace Server.Models;

[GenerateSerializer]
public class RoundResultModel
{
    [Id(0)] public int TargetNumber { get; set; }
    [Id(1)] public string WinnerPlayer { get; set; } = string.Empty;
    [Id(2)] public int WinnerNumber { get; set; }
}