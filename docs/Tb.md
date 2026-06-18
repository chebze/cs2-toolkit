# Triggerbot (TB)

## Purpose

Automatically fires when the crosshair is on an enemy, with humanized timing so shots can begin slightly before and end slightly after true crosshair contact.

## Toggle

| Setting | Default | Description |
|---------|---------|-------------|
| `Tb:ToggleKey` | `F7` | Tap to enable/disable TB |

### Controls

| Input | Action |
|-------|--------|
| **Tap** `F7` | Toggle TB on/off |
| **Hold** `F7` + `←` | Decrease pre-fire FOV |
| **Hold** `F7` + `→` | Increase pre-fire FOV |
| **Hold** `F7` + `↑` | Raise min/max reaction delay by 50ms |
| **Hold** `F7` + `↓` | Lower min/max reaction delay by 50ms |

FOV and reaction-delay changes apply immediately. Min reaction delay clamps to `0` ms; max reaction delay clamps to a minimum of `50` ms. Releasing `F7` after adjusting settings does **not** toggle TB.

TB starts **disabled**. The bottom-left overlay shows:

- **Green** `TB` — enabled (stacked above `RCS`)
- **Red** `TB` — disabled

## Configuration

`Toolkit:Tb` in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `ToggleKey` | `F7` | Hotkey toggle (tap on release) |
| `PreFireFovDegrees` | `0.7` | Starting angular window for early shots |
| `MinPreFireFovDegrees` | `0.1` | Minimum FOV when adjusting in-game |
| `MaxPreFireFovDegrees` | `5` | Maximum FOV when adjusting in-game |
| `FovAdjustStepDegrees` | `0.05` | FOV change per arrow press/repeat |
| `FovAdjustRepeatIntervalMs` | `80` | Repeat rate while holding `F7` + arrow |
| `MinGraceBullets` | `1` | Minimum early/late grace bullets |
| `MaxGraceBullets` | `2` | Maximum early/late grace bullets |
| `MinReactionDelayMs` | `200` | Minimum wait before firing on a new target |
| `MaxReactionDelayMs` | `400` | Maximum wait before firing on a new target |
| `ReactionDelayAdjustStepMs` | `50` | Reaction delay change per arrow press/repeat |
| `FovCircleColor` | `#EF4444` | Crosshair FOV ring color when TB is enabled |
| `FovCircleLineWidth` | `1.5` | Crosshair FOV ring stroke width |
| `AssumedHorizontalFovDegrees` | `90` | Used to map `PreFireFovDegrees` to screen pixels |

## Humanization

Each new target acquisition rolls a random reaction delay between `MinReactionDelayMs` and `MaxReactionDelayMs`. TB will not fire until that delay elapses, which reduces instant pre-fire when enemies peek from cover.

Each acquisition also rolls a random grace budget between `MinGraceBullets` and `MaxGraceBullets`:

- **Pre-fire**: while aiming near a spotted enemy (within `PreFireFovDegrees`) but not yet on target, fire up to that many bullets early (after the reaction delay)
- **On target**: fire while `m_iIDEntIndex` points at a living enemy (after the reaction delay)
- **Post-fire**: after leaving the target, fire up to that many extra bullets (no new reaction delay)

## Behavior

- Runs on the same **5ms** memory poll as other features
- Does not run while you are already holding left mouse
- Skips while reloading
- **On target** trusts the crosshair entity index (trace hit only)
- **Pre-fire** requires spotted-by-you visibility so it won't lead through walls
- When enabled in a match, draws a red FOV ring at the crosshair sized to `PreFireFovDegrees`, with min/max reaction delay shown to the right
- Uses synthetic mouse down/up via `SendInput`

## Memory reads

- `m_iIDEntIndex` — entity under crosshair
- `m_angEyeAngles` + `m_vecViewOffset` — aim direction for pre-fire detection
- Enemy pawn positions from the player list
- `m_entitySpottedState` / `bSpottedByMask` — enemy must be visible to you (line-of-sight), not just in aim FOV

## Overlay

- Layer: `tb-status`
- Position: bottom-left status text, one line above `RCS`
- Crosshair FOV ring: red circle centered on screen when TB is enabled in-match
- Reaction delay labels: `{MinReactionDelayMs} ms` and `{MaxReactionDelayMs} ms` to the right of the ring, vertically centered
- Visible after injection attaches to CS2
