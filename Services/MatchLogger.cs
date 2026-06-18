using Cs2Toolkit.Configuration;
using Cs2Toolkit.Events;
using Cs2Toolkit.Logging;
using Cs2Toolkit.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cs2Toolkit.Services;

public sealed class MatchLogger : IHostedService
{
    private readonly ToolkitEventBus _eventBus;
    private readonly FileLogWriter _fileLog;
    private readonly ToolkitOptions _options;
    private readonly ILogger<MatchLogger> _logger;

    private int _lastRoundStartCount = -1;
    private int _lastRoundEndCount = -1;
    private bool _lastInMatch;
    private (int EnemiesAlive, int EnemiesDead, int TeammatesAlive, int TeammatesDead) _lastStats;

    public MatchLogger(
        ToolkitEventBus eventBus,
        FileLogWriter fileLog,
        IOptions<ToolkitOptions> options,
        ILogger<MatchLogger> logger)
    {
        _eventBus = eventBus;
        _fileLog = fileLog;
        _options = options.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.FileLogging.Enabled)
        {
            _logger.LogInformation("MatchLogger disabled in configuration");
            return Task.CompletedTask;
        }

        _eventBus.OnMemoryRead += OnMemoryRead;
        _eventBus.OnInjectionStatusChanged += OnInjectionStatusChanged;
        _logger.LogInformation("MatchLogger writing to {FilePath}", _fileLog.FilePath);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _eventBus.OnMemoryRead -= OnMemoryRead;
        _eventBus.OnInjectionStatusChanged -= OnInjectionStatusChanged;
        return Task.CompletedTask;
    }

    private void OnInjectionStatusChanged(object? sender, InjectionStatusEventArgs e)
    {
        if (!_options.FileLogging.Enabled)
            return;

        _fileLog.Write("INJECTION", $"{e.Status}: {e.Message}");
    }

    private void OnMemoryRead(object? sender, MemoryReadEventArgs e)
    {
        if (!_options.FileLogging.Enabled)
            return;

        var state = e.State;
        if (!state.IsAttached)
            return;

        if (state.IsInMatch != _lastInMatch)
        {
            _fileLog.Write(
                state.IsInMatch ? "MATCH_ENTER" : "MATCH_EXIT",
                FormatMatchContext(state));

            _lastInMatch = state.IsInMatch;

            if (!state.IsInMatch)
            {
                ResetRoundTracking();
                return;
            }
        }

        if (!state.IsInMatch)
            return;

        DetectRoundTransitions(state);
        LogStatChanges(state);
    }

    private void DetectRoundTransitions(MemoryState state)
    {
        var round = state.Round;

        if (_lastRoundStartCount >= 0 && round.RoundStartCount != _lastRoundStartCount)
        {
            _fileLog.Write("ROUND_START", FormatRoundEvent(state));
            if (_options.FileLogging.LogPlayerDetailsOnRoundEvents)
                LogPlayers(state);
        }

        if (_lastRoundStartCount < 0)
            _fileLog.Write("ROUND_TRACKING", $"Initial round counters — start={round.RoundStartCount}, end={round.RoundEndCount}, total={round.TotalRoundsPlayed}");

        if (_lastRoundEndCount >= 0 && round.RoundEndCount != _lastRoundEndCount)
        {
            _fileLog.Write("ROUND_END", FormatRoundEndEvent(state));
            if (_options.FileLogging.LogPlayerDetailsOnRoundEvents)
                LogPlayers(state);
        }

        _lastRoundStartCount = round.RoundStartCount;
        _lastRoundEndCount = round.RoundEndCount;
    }

    private void LogStatChanges(MemoryState state)
    {
        if (!_options.FileLogging.LogStatChanges)
            return;

        var stats = (state.EnemiesAlive, state.EnemiesDead, state.TeammatesAlive, state.TeammatesDead);
        if (stats == _lastStats)
            return;

        _fileLog.Write(
            "STATS",
            $"enemies={state.EnemiesAlive}/{state.EnemiesDead} teammates={state.TeammatesAlive}/{state.TeammatesDead} " +
            $"roundStart={state.Round.RoundStartCount} roundEnd={state.Round.RoundEndCount} freeze={state.Round.IsFreezePeriod}");

        _lastStats = stats;
    }

    private void LogPlayers(MemoryState state)
    {
        foreach (var player in state.Players.OrderBy(p => p.Index))
        {
            var role = player.IsLocalPlayer ? "local" : player.Team == state.LocalTeam ? "teammate" : "enemy";
            _fileLog.Write(
                "PLAYER",
                $"idx={player.Index} role={role} team={FormatTeam(player.Team)} name={player.Name} alive={player.IsAlive} hp={player.Health}");
        }
    }

    private static string FormatMatchContext(MemoryState state)
    {
        return $"localTeam={FormatTeam(state.LocalTeam)} totalRounds={state.Round.TotalRoundsPlayed} " +
               $"warmup={state.Round.IsWarmupPeriod} phase={state.Round.GamePhase} players={state.Players.Count}";
    }

    private static string FormatRoundEvent(MemoryState state)
    {
        var round = state.Round;
        return $"startCount={round.RoundStartCount} totalPlayed={round.TotalRoundsPlayed} freeze={round.IsFreezePeriod} " +
               $"warmup={round.IsWarmupPeriod} phase={round.GamePhase} " +
               $"stats enemies={state.EnemiesAlive}/{state.EnemiesDead} teammates={state.TeammatesAlive}/{state.TeammatesDead}";
    }

    private static string FormatRoundEndEvent(MemoryState state)
    {
        var round = state.Round;
        return $"endCount={round.RoundEndCount} totalPlayed={round.TotalRoundsPlayed} winner={FormatTeam(round.RoundWinnerTeam)} " +
               $"winStatus={round.RoundWinStatus} freeze={round.IsFreezePeriod} " +
               $"stats enemies={state.EnemiesAlive}/{state.EnemiesDead} teammates={state.TeammatesAlive}/{state.TeammatesDead}";
    }

    private static string FormatTeam(int team) => team switch
    {
        GameOffsets.TeamTerrorist => "T",
        GameOffsets.TeamCounterTerrorist => "CT",
        _ => team.ToString()
    };

    private void ResetRoundTracking()
    {
        _lastRoundStartCount = -1;
        _lastRoundEndCount = -1;
        _lastStats = default;
    }
}
