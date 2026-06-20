using CS2Toolkit.Models;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Models.Tests;

public sealed class EnemyEspModeParserTests
{
    [Theory]
    [InlineData("disabled", EnemyEspMode.Disabled)]
    [InlineData("Disabled", EnemyEspMode.Disabled)]
    [InlineData("full", EnemyEspMode.Full)]
    [InlineData("FULL", EnemyEspMode.Full)]
    [InlineData("lastseen", EnemyEspMode.LastSeen)]
    [InlineData(null, EnemyEspMode.LastSeen)]
    [InlineData("", EnemyEspMode.LastSeen)]
    [InlineData("unknown", EnemyEspMode.LastSeen)]
    public void Parse_maps_values(string? input, EnemyEspMode expected) =>
        Assert.Equal(expected, EnemyEspModeParser.Parse(input));

    [Theory]
    [InlineData(EnemyEspMode.Disabled, "Disabled")]
    [InlineData(EnemyEspMode.Full, "Full")]
    [InlineData(EnemyEspMode.LastSeen, "LastSeen")]
    public void ToConfigValue_round_trips(EnemyEspMode mode, string expected) =>
        Assert.Equal(expected, EnemyEspModeParser.ToConfigValue(mode));
}
