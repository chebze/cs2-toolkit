# BombStatus

## Purpose

Represents the current bomb state published in `BombInfo.Status`.

## Values

| Value | Meaning |
|-------|---------|
| `None` | Unknown or not applicable |
| `Carried` | C4 is in a T player's inventory but not active |
| `Equipped` | C4 is the active weapon |
| `OnGround` | Bomb has been dropped (`m_bBombDropped`) |
| `Planting` | A T player is arming the C4 |
| `Planted` | Bomb is planted |
| `Defusing` | Bomb is being defused |
