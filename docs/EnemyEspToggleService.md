# EnemyEspToggleService

## Purpose

Hosted service that binds `Toolkit:EnemyEsp:ToggleKey` (default `F6`) to cycle `EnemyEspState.Mode`.

## Behavior

- Subscribes to `ToolkitEventBus.OnKeyPress`
- Logs each mode change
- Invalid toggle key throws at startup

## Configuration

`Toolkit:EnemyEsp:ToggleKey`, `Toolkit:EnemyEsp:Mode`
