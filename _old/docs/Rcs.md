# Recoil Compensation (RCS)

## Purpose

Reads aim punch from the local player each memory tick and moves the mouse in the opposite direction while spraying. Toggle on/off at runtime with a hotkey.

## Toggle

| Setting | Default | Description |
|---------|---------|-------------|
| `Rcs:ToggleKey` | `F8` | Press to enable/disable RCS |

RCS starts **disabled**. The bottom-left overlay label shows state:

- **Green** `RCS` — enabled
- **Red** `RCS` — disabled

## Configuration

`Toolkit:Rcs` in `appsettings.json`:

| Setting | Default | Description |
|---------|---------|-------------|
| `ToggleKey` | `F8` | Hotkey toggle |
| `Sensitivity` | `1.25` | Match your in-game mouse sensitivity |
| `PitchScale` | `2` | Vertical compensation multiplier |
| `YawScale` | `2` | Horizontal compensation multiplier |
| `FirstBulletCompensateChance` | `0.5` | Chance to compensate the first compensated bullet (shot 2) |
| `SubsequentBulletSkipChance` | `0.2` | Per-bullet chance to skip compensation after the first |

Tune `Sensitivity` to match CS2 settings if compensation feels too weak or too strong.

## Humanization

When `shotsFired` increases (a new bullet in the spray), a random roll decides whether that bullet is compensated:

- **First compensated bullet** (`shotsFired == 2`): compensated `FirstBulletCompensateChance` of the time (default 50%)
- **Later bullets**: skipped `SubsequentBulletSkipChance` of the time (default 20%)

Punch tracking still advances when compensation is skipped so the next compensated bullet does not over-correct.

## Memory reads

- `C_CSPlayerPawn::m_pAimPunchServices` → aim punch cache (`m_unpredictableBaseTick - 0x18`)
- Latest punch angle from the cache vector
- `m_iShotsFired` — only compensates after the second bullet
- `m_bIsScoped` — skipped while scoped

## Behavior

- Runs on the same **5ms** memory poll as other features
- Only active while **left mouse** is held
- Resets punch tracking when not firing, scoped, or disabled

## Overlay

- Layer: `rcs-status`
- Position: bottom-left (`Overlay:RcsStatus:Margin`)
- Visible after injection attaches to CS2
