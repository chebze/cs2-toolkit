# RoundInfo

## Purpose

Round state snapshot read from `C_CSGameRules` and included in each `MemoryState`.

## Properties

| Property | Source | Description |
|----------|--------|-------------|
| `TotalRoundsPlayed` | `m_totalRoundsPlayed` | Completed rounds in match |
| `RoundStartCount` | `m_nRoundStartCount` | Increments each round start |
| `RoundEndCount` | `m_nRoundEndCount` | Increments each round end |
| `IsFreezePeriod` | `m_bFreezePeriod` | Buy freeze active |
| `IsWarmupPeriod` | `m_bWarmupPeriod` | Warmup active |
| `GamePhase` | `m_gamePhase` | Current game phase enum value |
| `RoundWinStatus` | `m_iRoundWinStatus` | Round outcome status |
| `RoundWinnerTeam` | `m_iRoundEndWinnerTeam` | Winning team (2=T, 3=CT) |

## Usage

`MatchLogger` compares `RoundStartCount` and `RoundEndCount` between reads to emit `ROUND_START` and `ROUND_END` log events.

## Static instance

`RoundInfo.Empty` — default when game rules are unavailable.
