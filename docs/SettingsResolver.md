# SettingsResolver

## Purpose

Merges layered weapon settings (global → weapon type → weapon id) for triggerbot, RCS, and aim helper.

## Key API

Implements `ISettingsResolver`.

## Behavior

Uses nullable overlay merge: only non-null properties from more specific layers override broader layers.

## Dependencies

`WeaponCatalog` for weapon category keys.

## Configuration

Layer data stored per profile in `ConfigurationStore`.
